import numpy as np
import torch
import torch.nn.functional as F
import random
from tqdm.notebook import trange

from mcts import Node
from constants import PLAYER1


class TrainingMCTS:
    def __init__(self, game, args, model):
        self.game = game
        self.args = args
        self.model = model

    @torch.no_grad()
    def search(self, states, self_play_games):
        policy, _ = self.model(
            torch.tensor(self.game.get_encoded_state(states), device=self.model.device)
        )
        policy = torch.softmax(policy, axis=1).cpu().numpy()

        for i, game in enumerate(self_play_games):
            game_policy = policy[i]
            valid_moves = self.game.get_moves(states[i])
            game_policy *= valid_moves
            game_policy /= np.sum(game_policy)

            game.root = Node(self.game, self.args, states[i])
            game.root.expand(game_policy)

        for _ in range(self.args["num_searches"]):
            for game in self_play_games:
                game.node = None
                node = game.root

                while node.is_fully_expanded():
                    node = node.select()

                value, terminated = self.game.get_value_and_terminated(
                    node.state, node.action
                )
                value = self.game.get_opponent_value(value)

                if terminated:
                    node.backpropagate(value)
                else:
                    game.node = node

            expandable_games = [
                idx
                for idx in range(len(self_play_games))
                if self_play_games[idx].node is not None
            ]
            if len(expandable_games) > 0:
                states = np.stack(
                    [self_play_games[idx].node.state for idx in expandable_games]
                )
                policy, value = self.model(
                    torch.tensor(
                        self.game.get_encoded_state(states), device=self.model.device
                    )
                )
                policy = torch.softmax(policy, axis=1).detach().cpu().numpy()
                value = value.cpu().numpy()

            for i, idx in enumerate(expandable_games):
                node = self_play_games[idx].node
                self_play_policy, self_play_value = policy[i], value[i]

                valid_moves = self.game.get_moves(node.state)
                self_play_policy = self_play_policy * valid_moves
                self_play_policy /= np.sum(self_play_policy)
                node.expand(self_play_policy)
                node.backpropagate(self_play_value)


class SelfPlayGame:
    def __init__(self, game):
        self.state = game.get_initial_state()
        self.memory = []
        self.root = None
        self.node = None


class AlphaZero:
    def __init__(self, model, optimizer, game, args):
        self.model = model
        self.optimizer = optimizer
        self.game = game
        self.args = args
        self.mcts = TrainingMCTS(game, args, model)

    def self_play(self):
        return_memory = []
        player = PLAYER1
        self_paly_games = [
            SelfPlayGame(self.game) for _ in range(self.args["num_parallel_games"])
        ]

        while len(self_paly_games) > 0:
            states = np.stack([game.state for game in self_paly_games])
            neutral_states = self.game.change_perspective(states, player)
            self.mcts.search(neutral_states, self_paly_games)

            for i in range(len(self_paly_games))[::-1]:
                game = self_paly_games[i]

                action_probs = np.zeros(self.game.size * self.game.size)
                for child in game.root.children:
                    action_probs[child.action] = child.visit_count
                action_probs = action_probs / np.sum(action_probs)

                game.memory.append((game.root.state, action_probs, player))
                action = np.random.choice(
                    self.game.size * self.game.size, p=action_probs
                )
                game.state = self.game.get_next_state(game.state, action, player)
                value, terminated = self.game.get_value_and_terminated(
                    game.state, action
                )
                if terminated:
                    for state, action_probs, player in game.memory:
                        outcome = (
                            value
                            if player == PLAYER1
                            else self.game.get_opponent_value(value)
                        )
                        return_memory.append(
                            (self.game.get_encoded_state(state), action_probs, outcome)
                        )
                    del self_paly_games[i]

            player = self.game.get_opponent(player)

        return return_memory

    def train(self, memory):
        random.shuffle(memory)
        for batch_index in range(0, len(memory), self.args["batch_size"]):
            batch = memory[
                batch_index : min(len(memory) - 1, batch_index)
                + self.args["batch_size"]
            ]
            states, policy_targets, value_targets = zip(*batch)

            states, policy_targets, value_targets = (
                np.array(states),
                np.array(policy_targets),
                np.array(value_targets).reshape(-1, 1),
            )
            states = torch.tensor(states, dtype=torch.float32, device=self.model.device)
            policy_targets = torch.tensor(
                policy_targets, dtype=torch.float32, device=self.model.device
            )
            value_targets = torch.tensor(
                value_targets, dtype=torch.float32, device=self.model.device
            )

            out_policy, out_value = self.model(states)

            policy_loss = F.cross_entropy(out_policy, policy_targets)
            value_loss = F.mse_loss(out_value, value_targets)
            loss = policy_loss + value_loss

            self.optimizer.zero_grad()
            loss.backward()
            self.optimizer.step()

    def learn(self):
        for iteration in trange(self.args["num_iterations"]):
            memory = []

            self.model.eval()
            for play_iteration in trange(
                self.args["num_self_play_iterations"] // self.args["num_parallel_games"]
            ):
                memory += self.self_play()

            self.model.train()
            for epoch in range(self.args["num_epochs"]):
                self.train(memory)

            torch.save(
                self.model.state_dict(),
                f'{self.args["model_path"]}/model_{iteration}.pt',
            )
            torch.save(
                self.optimizer.state_dict(),
                f'{self.args["optimizer_path"]}/optimizer_{iteration}.pt',
            )
