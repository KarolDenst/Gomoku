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
        if action is None:
            return False

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

    def get_value_and_terminated(self, state, action):
        if self.check_win(state, action):
            return 1, True
        if np.sum(state == EMPTY) == 0:
            return 0, True
        return 0, False

    def get_opponent(self, player):
        return PLAYER1 if player == PLAYER2 else PLAYER2

    def get_opponent_value(self, player):
        return -player

    def change_perspective(self, state, player):
        return state * player

    def get_encoded_state(self, state):
        encoded_state = np.stack(
            (state == PLAYER1, state == PLAYER2, state == EMPTY)
        ).astype(np.float32)

        if len(state.shape) == 3:
            encoded_state = np.swapaxes(encoded_state, 0, 1)
        
        return encoded_state


    def print(self, state):
        board_str = ''
        for row in range(self.size):
            row_str = ' '.join(str(state[row, col]) if state[row, col] != EMPTY else '.' for col in range(self.size))
            board_str += row_str + '\n'
        board_str = board_str.replace('-1', 'O').replace('1', 'X')
        print(board_str)