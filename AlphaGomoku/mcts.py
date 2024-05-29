import numpy as np
import torch
from constants import PLAYER1, PLAYER2


class Node:
    def __init__(self, game, args, state, parent=None, action=None, prior=0):
        self.game = game
        self.args = args
        self.state = state
        self.parent = parent
        self.action = action
        self.prior = prior

        self.children = []
        self.visit_count = 0
        self.total_value = 0

    def is_fully_expanded(self):
        """
        Checks if the node is fully expanded.

        Returns:
            bool: True if the node is fully expanded, False otherwise.
        """
        return len(self.children) > 0

    def select(self):
        """
        Selects the best child node based on the Upper Confidence Bound (UCB) value.

        Returns:
            The best child node.
        """
        best_child = None
        best_ucb = -np.inf
        for child in self.children:
            ucb = self.get_ucb(child)
            if ucb > best_ucb:
                best_ucb = ucb
                best_child = child

        return best_child

    def get_ucb(self, child):
        """
        Calculates the Upper Confidence Bound (UCB) value for a given child node.

        Parameters:
            child (Node): The child node for which to calculate the UCB value.

        Returns:
            float: The UCB value for the child node.
        """
        if child.visit_count == 0:
            q_value = 0
        else:
            q_value = 1 - (child.total_value / child.visit_count + 1) / 2
        return (
            q_value
            + self.args["C"]
            * np.sqrt(self.visit_count)
            / (child.visit_count + 1)
            * child.prior
        )

    def expand(self, policy):
        """
        Expands the current node by creating child nodes for each possible action.

        Args:
            policy (list): A list of probabilities for each possible action.

        Returns:
            child (Node): The child node that was created.
        """
        for action, prob in enumerate(policy):
            if prob > 0:
                child_state = self.state.copy()
                child_state = self.game.get_next_state(child_state, action, PLAYER1)
                child_state = self.game.change_perspective(child_state, player=PLAYER2)

                child = Node(self.game, self.args, child_state, self, action, prob)
                self.children.append(child)

        return child

    def backpropagate(self, value):
        """
        Backpropagate the value obtained from the simulation to update the statistics of the current node and its ancestors.

        Args:
            value: The value obtained from the simulation.

        Returns:
            None
        """
        self.visit_count += 1
        self.total_value += value
        if self.parent is not None:
            value = self.game.get_opponent_value(value)
            self.parent.backpropagate(value)


class AlphaMCTS:
    def __init__(self, game, args, model):
        self.game = game
        self.args = args
        self.model = model

    @torch.no_grad()
    def search(self, state):
        """
        Performs Monte Carlo Tree Search (MCTS) to find the best action to take.

        Args:
            state (np.array): The current state of the game.

        Returns:
            np.array: An array of action probabilities, indicating the likelihood of selecting each action.
        """
        root = Node(self.game, self.args, state)
        for _ in range(self.args["num_searches"]):
            node = root

            # selection
            while node.is_fully_expanded():
                node = node.select()

            value, terminated = self.game.get_value_and_terminated(
                node.state, node.action
            )
            value = self.game.get_opponent_value(value)

            if not terminated:
                policy, value = self.model(
                    torch.tensor(
                        self.game.get_encoded_state(node.state),
                        device=self.model.device,
                    ).unsqueeze(0)
                )
                policy = torch.softmax(policy, axis=1).squeeze(0).detach().cpu().numpy()
                valid_moves = self.game.get_moves(node.state)
                policy = policy * valid_moves
                policy /= np.sum(policy)

                value = value.item()
                node.expand(policy)

            node.backpropagate(value)

        action_probs = np.zeros(self.game.size * self.game.size)
        for child in root.children:
            action_probs[child.action] = child.visit_count
        action_probs = action_probs / np.sum(action_probs)

        return action_probs
