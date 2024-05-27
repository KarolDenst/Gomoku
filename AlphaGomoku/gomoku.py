import numpy as np
EMPTY = 0
PLAYER1 = 1
PLAYER2 = -1

class Gomoku:
    def __init__(self, size=15):
        self.size = size

    def get_initial_state(self):
        return np.zeros((self.size, self.size)).astype(np.int8)

    def get_next_state(self, state, action, player):
        row = action // self.size
        col = action % self.size
        if state[row, col] != EMPTY:
            raise ValueError("Invalid action")
        state[row, col] = player
        return state
    
    def get_moves(self, state):
        return (state.reshape(-1) == EMPTY).astype(np.uint8)

    def check_win(self, state, action):
        row = action // self.size
        col = action % self.size
        player = state[row, col]
        if player == EMPTY:
            return False

        directions = [(1, 0), (0, 1), (1, 1), (1, -1)]

        for dr, dc in directions:
            count = 1

            # Create an array of indices for positive direction
            indices = np.array([(row + i * dr, col + i * dc) for i in range(1, 5)])
            valid_indices = (indices[:, 0] >= 0) & (indices[:, 0] < self.size) & (indices[:, 1] >= 0) & (indices[:, 1] < self.size)
            valid_indices = indices[valid_indices]

            count += np.sum(state[valid_indices[:, 0], valid_indices[:, 1]] == player)

            # Create an array of indices for negative direction
            indices = np.array([(row - i * dr, col - i * dc) for i in range(1, 5)])
            valid_indices = (indices[:, 0] >= 0) & (indices[:, 0] < self.size) & (indices[:, 1] >= 0) & (indices[:, 1] < self.size)
            valid_indices = indices[valid_indices]

            count += np.sum(state[valid_indices[:, 0], valid_indices[:, 1]] == player)

            if count >= 5:
                return True

        return False
    
    def get_decisive_and_terminated(self, state, action):
        if self.check_win(state, action):
            return True, True
        if np.sum(state == EMPTY) == 0:
            return False, True
        return False, False
    
    def get_opponent(self, player):
        return PLAYER1 if player == PLAYER2 else PLAYER2
    
    def print(self, state):
        board_str = ''
        for row in range(self.size):
            row_str = ' '.join(str(state[row, col]) if state[row, col] != EMPTY else '.' for col in range(self.size))
            board_str += row_str + '\n'
        print(board_str)
        
game = Gomoku()
player = PLAYER1

state = game.get_initial_state()

while True:
    game.print(state)
    valid_moves = game.get_moves(state)
    print("Valid moves:", [i for i in range(game.size * game.size) if valid_moves[i] == 1])
    action = int(input("Enter action: "))
    
    if valid_moves[action] == 0:
        print("Invalid move")
        continue
    
    state = game.get_next_state(state, action, player)
    decisive, terminated = game.get_decisive_and_terminated(state, action)
    if terminated:
        if decisive == 1:
            print("Player", player, "wins")
        else:
            print("Draw")
        break

    player = game.get_opponent(player)