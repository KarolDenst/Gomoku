import numpy as np
from constants import PLAYER1, PLAYER2, EMPTY


class Gomoku:
    """
    Static class that contains methods related to Gomoku.
    """
    
    def __init__(self, size=15):
        self.size = size

    def get_initial_state(self):
        """
        Create an empty board.

        Returns:
            np.array: A 2D array representing the board.
        """
        return np.zeros((self.size, self.size)).astype(np.int8)

    def get_next_state(self, state, action, player):
        """
        Creates a new state by applying the action to the current state.

        Args:
            state (np.array): The current state of the game.
            action (int): The action to apply to the state. Represented as an integer from 0 to size^2 - 1.
            player (int): The player to apply the action. Should be either PLAYER1 or PLAYER2.

        Raises:
            ValueError: The action is invalid. The cell is already occupied.

        Returns:
            np.array: A 2D array representing the new state of the game.
        """
        row = action // self.size
        col = action % self.size
        if state[row, col] != EMPTY:
            raise ValueError("Invalid action")
        state[row, col] = player
        return state

    def get_moves(self, state):
        """
        Get all the legal actions for the current state.

        Args:
            state (np.array): The current state of the game.

        Returns:
            np.array: A 1D array of size size^2 representing the legal actions.
        """
        return (state.reshape(-1) == EMPTY).astype(np.uint8)

    def check_win(self, state, action):
        """
        Checks if the action won the game.

        Args:
            state (np.array): The current state of the game.
            action (int): Last action made.

        Returns:
            boolean: True if the action won the game, False otherwise.
        """
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
            valid_indices = (
                (indices[:, 0] >= 0)
                & (indices[:, 0] < self.size)
                & (indices[:, 1] >= 0)
                & (indices[:, 1] < self.size)
            )
            valid_indices = indices[valid_indices]

            count += np.sum(state[valid_indices[:, 0], valid_indices[:, 1]] == player)

            # Create an array of indices for negative direction
            indices = np.array([(row - i * dr, col - i * dc) for i in range(1, 5)])
            valid_indices = (
                (indices[:, 0] >= 0)
                & (indices[:, 0] < self.size)
                & (indices[:, 1] >= 0)
                & (indices[:, 1] < self.size)
            )
            valid_indices = indices[valid_indices]

            count += np.sum(state[valid_indices[:, 0], valid_indices[:, 1]] == player)

            if count >= 5:
                return True

        return False

    def get_value_and_terminated(self, state, action):
        """Returns the value of the state and whether the game is terminated.

        Args:
            state (np.array): The current state of the game.
            action (int): The last action made.

        Returns:
            (int, boolean): The value of the state and whether the game is terminated.
        """
        if self.check_win(state, action):
            return 1, True
        if np.sum(state == EMPTY) == 0:
            return 0, True
        return 0, False

    def get_opponent(self, player):
        """
        Returns the opponent of the given player.

        Args:
            player (int): The current player.

        Returns:
            int: The opponent.
        """
        return PLAYER1 if player == PLAYER2 else PLAYER2

    def get_opponent_value(self, player):
        """
        _summary_

        Args:
            player (_type_): _description_

        Returns:
            _type_: _description_
        """
        return -player

    def change_perspective(self, state, player):
        """
        Returns the state from the perspective of the given player. Also works for batched states.

        Args:
            state (np.array): The current state of the game.
            player (int): The player to change the perspective to.

        Returns:
            np.array: The new state of the game.
        """
        return state * player

    def get_encoded_state(self, state):
        """
        Encodes the game state into a multi-channel array.

        Args:
            state (np.array): The game state represented as a 2D or 3D numpy array.

        Returns:
            np.array: The encoded game state as a multi-channel array.
        """
        encoded_state = np.stack(
            (state == PLAYER1, state == PLAYER2, state == EMPTY)
        ).astype(np.float32)

        if len(state.shape) == 3:
            encoded_state = np.swapaxes(encoded_state, 0, 1)

        return encoded_state

    def print(self, state):
        """
        Prints the current state of the Gomoku board.

        Args:
            state (np.array): The current state of the Gomoku board.

        Returns:
            None
        """
        board_str = ""
        for row in range(self.size):
            row_str = " ".join(
                str(state[row, col]) if state[row, col] != EMPTY else "."
                for col in range(self.size)
            )
            board_str += row_str + "\n"
        board_str = board_str.replace("-1", "O").replace("1", "X")
        print(board_str)
