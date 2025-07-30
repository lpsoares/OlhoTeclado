class BaseDecoder:
    def __init__(self):
        self.points = []
        self.context = ""

    def update_layout(
        self, layout: dict[str, tuple[float, float, float, float]]
    ) -> None:
        """
        Update the keyboard layout for the decoder.
        :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
        """
        pass  # Override in subclasses with specific layout handling

    def add_points(self, points: list[tuple[float, float, float]]):
        """
        Add points to the decoder.
        :param points: A list of tuples representing points (timestamp, x, y)
        """
        self.points.extend(points)

    def reset_points(self):
        """
        Reset the points in the decoder.
        """
        self.points = []

    def set_context(self, context: str):
        """
        Set the context for the decoder.
        :param context: The context string to set
        """
        self.context = context

    def decode_word(self, top_n: int = 5) -> list[str]:
        """
        Decode the gesture points and return the top N candidates.
        :param top_n: The number of top candidates to return
        :return: A list of decoded words
        """
        raise NotImplementedError("Subclasses should implement this method")
