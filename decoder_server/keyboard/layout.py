import csv
from os import PathLike


class KeyboardButton:
    def __init__(self, key_id: str, cx: float, cy: float, width: float, height: float):
        self.key_id = key_id
        self.cx = cx
        self.cy = cy
        self.width = width
        self.height = height

    @property
    def x(self) -> float:
        """
        Returns the x coordinate of the button.
        :return: The x coordinate.
        """
        return self.cx - self.width / 2

    @property
    def y(self) -> float:
        """
        Returns the y coordinate of the button.
        :return: The y coordinate.
        """
        return self.cy - self.height / 2

    @property
    def center(self) -> tuple[float, float]:
        """
        Returns the center coordinates of the button.
        :return: A tuple containing the x and y coordinates of the center.
        """
        return self.cx, self.cy

    @property
    def normalized_center(self) -> tuple[float, float]:
        """
        Returns the normalized center coordinates of the button.
        :return: A tuple containing the normalized x and y coordinates of the center.
        """
        return self.cx / self.key_size, self.cy / self.key_size

    @property
    def key_size(self) -> float:
        """
        Returns the size of the key.
        :return: The size of the key.
        """
        return min(self.width, self.height)

    @property
    def key(self) -> str:
        """
        Returns the key associated with the button.
        :return: The key associated with the button.
        """
        return self.key_id[:-3]


class KeyboardLayout:
    def __init__(self, layout_file: PathLike | None = None):
        self.buttons = {}
        if layout_file:
            self.from_file(layout_file)

    @property
    def is_initialized(self) -> bool:
        """
        Returns True if the keyboard layout is initialized, False otherwise.
        :return: True if initialized, False otherwise.
        """
        return bool(self.buttons)

    @property
    def key_size(self) -> float:
        return self.buttons["aKey"].key_size

    def from_file(self, layout_file: PathLike) -> None:
        """
        Loads a keyboard layout from a file.
        :param layout_file: The path to the file containing the keyboard layout.
        """
        with open(layout_file, "r", encoding="utf-8") as f:
            reader = csv.reader(f)
            for row in reader:
                _tstamp, key_id, *numeric_data = row
                x, y, width, height = map(float, numeric_data)
                button = KeyboardButton(key_id, x, y, width, height)
                self.buttons[key_id] = button

    def from_keyboard_config(
        self, keyboard_config: dict[str, tuple[float, float, float, float]]
    ) -> None:
        """
        Loads a keyboard layout from a dictionary.
        :param keyboard_config: A dictionary containing the keyboard layout.
        """
        for key_id, (x, y, width, height) in keyboard_config.items():
            if not key_id.endswith("Key"):
                key_id = key_id.lower() + "Key"
            button = KeyboardButton(key_id, x, y, width, height)
            self.buttons[key_id] = button

    def ideal_path_for(self, word: str) -> list[tuple[float, float]]:
        """
        Returns the ideal path for typing a word on the keyboard.
        :param word: The word to type.
        :return: A list of tuples containing the x and y coordinates of the buttons.
        """
        path = []
        for char in word.lower():
            button = self[char]
            if button:
                path.append(button.center)
        return path

    def get_closest_key(
        self, x: float, y: float, threshold: float
    ) -> KeyboardButton | None:
        """
        Returns the closest button to the given coordinates.
        :param x: The x coordinate.
        :param y: The y coordinate.
        :param threshold: The distance threshold to consider a button as close (value will be multiplied by key_size).
        :return: The closest button or None if no button is close enough.
        """
        closest_button = None
        min_distance = float("inf")
        for button in self.buttons.values():
            distance = ((button.cx - x) ** 2 + (button.cy - y) ** 2) ** 0.5
            if distance < min_distance:
                min_distance = distance
                closest_button = button
        return closest_button if min_distance < threshold * self.key_size else None

    def keys_close_to(
        self, x: float, y: float, threshold: float
    ) -> list[KeyboardButton]:
        """
        Returns a list of buttons that are close to the given coordinates.
        :param x: The x coordinate.
        :param y: The y coordinate.
        :param threshold: The distance threshold.
        :return: A list of buttons that are close to the given coordinates.
        """
        close_buttons = []
        for button in self.buttons.values():
            distance = ((button.cx - x) ** 2 + (button.cy - y) ** 2) ** 0.5
            if distance < threshold:
                close_buttons.append(button)
        return close_buttons

    def __getitem__(self, key: str) -> KeyboardButton:
        """
        Returns the button corresponding to the given key.
        :param key: The key to look up.
        :return: The button corresponding to the key.
        """
        return self.buttons[key.lower() + "Key"]
