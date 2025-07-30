import math

from BaseDecoder import BaseDecoder
from keyboard.layout import KeyboardLayout
from language.language_model import GPT2LanguageModel

# Constants
RELEASE = 0
HOLD = 1

# Gaussian parameters
GAUSSIAN_STD = 0.4  # Empirically set
STABILITY_WINDOW_SIZE = 1000  # (milliseconds) For stability score
HOLD_TIME = 1000  # (milliseconds) Time to hold a key before releasing


class GlanceWriterDecoder(BaseDecoder):
    def __init__(
        self,
        lexicon: list[str],
        keyboard_config: dict[str, tuple[float, float, float, float]] | None = None,
        gaussian_std: float = GAUSSIAN_STD,
        stability_window_size: int = STABILITY_WINDOW_SIZE,
        hold_time: int = HOLD_TIME,
    ):
        super().__init__()
        self.lexicon = lexicon
        self.gaussian_std = gaussian_std
        self.stability_window_size = stability_window_size
        self.hold_time = hold_time
        self.language_model = GPT2LanguageModel()
        self.root = self._build_trie(lexicon)
        self.held_nodes = set()
        self.word_candidates: dict[str, WordCandidate] = {}
        self._reset()
        self.processing_context = False

        self.keyboard = KeyboardLayout()
        if keyboard_config:
            self.keyboard.from_keyboard_config(keyboard_config)

    def update_layout(
        self, layout: dict[str, tuple[float, float, float, float]]
    ) -> None:
        """
        Update the keyboard layout for the decoder.
        :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
        """
        if not self.keyboard:
            self.keyboard = KeyboardLayout()
        self.keyboard.from_keyboard_config(layout)
        self.root = self._build_trie(self.lexicon)

    def decode_word(
        self,
        top_n: int = 5,
    ) -> list[str]:
        assert self.keyboard.is_initialized

        while self.processing_context:
            print("Waiting for context processing to finish...")

        # Sort word_candidates by sum_score and output top 10
        top_candidates = [
            candidate
            for _, candidate in sorted(
                self.word_candidates.items(), key=lambda x: x[1].score, reverse=True
            )[: top_n * 2]
        ]
        print(f"Top candidates: {[candidate.word for candidate in top_candidates]}")

        # Combine with language model (we don't really need to divide by the total because we only care about the relative probabilities)
        results = []
        context = self.context.strip()
        predictions = None
        if context:
            self.language_model.set_words(
                [candidate.word for candidate in top_candidates]
            )
            predictions = self.language_model.predict_next_word(context)

        for candidate in top_candidates:
            lm_score = predictions.get(candidate.word, 0.0) if predictions else 1.0
            prob = candidate.score * lm_score
            results.append((candidate.word, prob))
        results.sort(key=lambda x: x[1], reverse=True)
        print(f"Results: {results}")

        return [word for word, _ in results[:top_n]]

    def add_points(self, points: list[tuple[float, float, float]]):
        """
        Add points to the decoder.
        :param points: A list of tuples representing points (timestamp, x, y)
        """
        for point in points:
            tstamp, x, y = point
            normalized_point = tstamp, *self.keyboard.normalize_point((x, y))
            self.points.append(normalized_point)
            self._update_point(normalized_point)

    def set_context(self, context: str):
        self.processing_context = True
        super().set_context(context)
        self.language_model.preprocess_sentence(context)
        self.processing_context = False

    def reset_points(self):
        super().reset_points()
        self._reset()

    def _reset(self):
        self.held_nodes = set()
        self.root = self._build_trie(self.lexicon)
        self.word_candidates = {}

    def _build_trie(self, lexicon):
        root = TrieNode("")
        for word in lexicon:
            node = root
            prev_char = ""
            for char in word:
                # Merge consecutive identical letters
                if char == prev_char:
                    continue
                if char not in node.children:
                    node.children[char] = TrieNode(char)
                node = node.children[char]
                prev_char = char
            node.words.append(word)
        return root

    def _update_point(self, point: tuple[float, float, float]):
        """
        Update the decoder with a new point.
        :param point: A tuple (timestamp, x, y)
        """
        root_children = list(self.root.children.values())
        tstamp, x, y = point

        self._trim_held_nodes(50)
        key = self.keyboard.get_closest_key(x, y, 1.5, True)
        if not key:
            return

        key_score = self._key_score(key.normalized_center)

        # Scan the first letter of each word
        for child in root_children:
            if child.char == key.key and child not in self.held_nodes:
                child.key_score = key_score
                child.sum_score = key_score
                self.held_nodes.add(child)

        # Scan each node in HOLD list
        for node in self.held_nodes.copy():
            if node.char == key.key:
                if key_score > node.key_score:
                    node.update_key_score(key_score)
            # Scan next letters
            for next_char, next_node in node.children.items():
                if next_char == key.key:
                    next_node.key_score = key_score
                    next_node.sum_score = node.sum_score + key_score
                    self.held_nodes.add(next_node)

            self._remove_old_candidates()
            # If node is last letter of words, add to word_candidates
            if node.words:
                for w in node.words:
                    if w not in self.word_candidates:
                        self.word_candidates[w] = WordCandidate(
                            w, node.sum_score, tstamp
                        )

    def _remove_old_candidates(self):
        """
        Remove old word candidates that are older than HOLD_TIME.
        """
        if not self.points:
            return
        current_time = self.points[-1][0]
        for word, candidate in list(self.word_candidates.items()):
            if current_time - candidate.timestamp > self.hold_time:
                del self.word_candidates[word]

    def _trim_held_nodes(self, max_size):
        """
        Trim the held nodes to a maximum size.
        :param max_size: Maximum number of held nodes to keep
        """
        if len(self.held_nodes) > max_size:
            self.held_nodes = set(
                sorted(self.held_nodes, key=lambda n: n.sum_score, reverse=True)[
                    :max_size
                ]
            )

    def _key_score(self, key_center):
        return self._distance_score(key_center) * self._stability_score()

    def _distance_score(self, key_center):
        if not self.points:
            return 0.0
        gaze_point = self.points[-1]
        dx = gaze_point[1] - key_center[0]
        dy = gaze_point[2] - key_center[1]
        dist = math.sqrt(dx * dx + dy * dy)
        return gaussian(dist, self.gaussian_std)

    def _stability_score(self):
        window = self._get_window(self.stability_window_size)
        if not window:
            return 1.0
        total_distance = 0.0
        for i in range(1, len(window)):
            dx = window[i][1] - window[i - 1][1]
            dy = window[i][2] - window[i - 1][2]
            dist = math.sqrt(dx * dx + dy * dy)
            total_distance += dist
        dt = window[-1][0] - window[0][0]  # Time difference in milliseconds
        if dt <= 0:
            return 1.0
        avg_speed = total_distance / dt
        return 1.0 / avg_speed

    def _get_window(self, delta_t):
        """
        Get the points in the window based on the last point's timestamp.
        :param delta_t: size of the window in milliseconds
        :return: the points within the window
        """
        window = []
        if not self.points:
            return window
        last_timestamp = self.points[-1][0]
        start_time = last_timestamp - delta_t
        for p in reversed(self.points):
            if p[0] < start_time:
                break
            window.append(p)
        return list(reversed(window))


class TrieNode:
    def __init__(self, char):
        self.char = char
        self.children = {}
        self.key_score = 0.0
        self.sum_score = 0.0
        self.words = []  # List of words ending here

    def update_key_score(self, key_score):
        """
        Update the key score for this node.
        :param key_score: The new score to set
        """
        self.sum_score = self.sum_score - self.key_score + key_score
        self.key_score = key_score


def gaussian(distance, std=GAUSSIAN_STD):
    return (1 / (std * math.sqrt(2 * math.pi))) * math.exp(
        -(distance**2) / (2 * std**2)
    )


class WordCandidate:
    def __init__(self, word: str, score: float, timestamp: float):
        self.word = word
        self.score = score
        self.timestamp = timestamp
