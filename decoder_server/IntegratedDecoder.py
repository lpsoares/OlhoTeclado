import time
from dataclasses import dataclass
from typing import Callable

from decoder import SuffixGestureDecoder
from iterdata import WordData
from language.language_model import GPT2LanguageModel
from probability_combiners import linear_combiner


@dataclass
class IntegratedWordScore:
    word: str
    gesture_probability: float
    language_probability: float
    combined_probability: float
    gesture_distance: float = 0.0


class IntegratedDecoder:
    def __init__(
        self,
        gesture_weight: float = 0.8,
        language_weight: float = 0.2,
        combine_x: float = 0.8,
        combiner: Callable[
            [float, float, float], Callable[[float, float], float]
        ] = linear_combiner,
        keyboard_config: dict[str, tuple[float, float, float, float]] | None = None,
    ):
        self.suffix_decoder = SuffixGestureDecoder(keyboard_config=keyboard_config)
        self.language_model = GPT2LanguageModel()
        self.combiner_fn = combiner(gesture_weight, language_weight, combine_x)
        self.gesture_weight = gesture_weight
        self.language_weight = language_weight
        self.combine_x = combine_x

    def update_layout(
        self, layout: dict[str, tuple[float, float, float, float]]
    ) -> None:
        """
        Update the keyboard layout for the decoder.
        :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
        """
        self.suffix_decoder.update_layout(layout)

    def decode_word(
        self,
        gesture_points: list[tuple[float, float, float]],
        context: str,
        top_n: int = 10,
    ) -> list[IntegratedWordScore]:
        gesture_candidates = self.suffix_decoder.decode(gesture_points)

        language_probs = {}
        if context.strip():
            start_time = time.time()
            # QUESTION: This method uses the added words, shouldn't we add the words before this?
            # QUESTION: The context doesn't end with a space, shouldn't we add one so the prediction doesn't try to continue the last word?
            predictions = self.language_model.predict_next_word(context)
            end_time = time.time()
            print(
                f"Language model prediction took {end_time - start_time:.4f} seconds for context: '{context}'"
            )
            for candidate in gesture_candidates[:50]:
                # QUESTION: Shouldn't we remove the previous words from the language model?
                self.language_model.add_word(candidate.word)

                # QUESTION: Why is this inside the loop? Do we need to do this for all candidates even though we only added the first 50?
                for candidate in gesture_candidates:
                    word = candidate.word
                    language_probs[word] = predictions.get(word, 0.0)

            for candidate in gesture_candidates[:10]:
                print(candidate.word, predictions.get(candidate.word, 0.0))
        else:
            uniform_prob = 1.0 / len(gesture_candidates) if gesture_candidates else 0
            language_probs = {
                candidate.word: uniform_prob for candidate in gesture_candidates
            }

        integrated_scores = [
            IntegratedWordScore(
                word=c.word,
                gesture_probability=c.probability,
                language_probability=language_probs.get(c.word, 0.0),
                combined_probability=self.combiner_fn(
                    c.probability, language_probs.get(c.word, 0.0)
                ),
                gesture_distance=getattr(c, "distance", 0.0),
            )
            for c in gesture_candidates
        ]

        integrated_scores.sort(key=lambda x: x.combined_probability, reverse=True)
        return integrated_scores[:top_n]

    def extract_sentence_context(self, word_data: WordData) -> str:
        words = word_data.target_sentence.split()
        if word_data.word_index <= 1:
            return ""

        return "".join(words[: word_data.word_index - 1])
