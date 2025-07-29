from dataclasses import dataclass
from typing import Callable

from BaseDecoder import BaseDecoder
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


class IntegratedDecoder(BaseDecoder):
    def __init__(
        self,
        decoder_id: str,
        gesture_weight: float = 0.8,
        language_weight: float = 0.2,
        combine_x: float = 0.8,
        combiner: Callable[
            [float, float, float], Callable[[float, float, int], float]
        ] = linear_combiner,
        keyboard_config: dict[str, tuple[float, float, float, float]] | None = None,
    ):
        super().__init__(decoder_id=decoder_id)
        self.suffix_decoder = SuffixGestureDecoder(keyboard_config=keyboard_config)
        self.language_model = GPT2LanguageModel()
        self.combiner_fn = combiner(gesture_weight, language_weight, combine_x)
        self.gesture_weight = gesture_weight
        self.language_weight = language_weight
        self.combine_x = combine_x
        self.context = ""

    def update_layout(
        self, layout: dict[str, tuple[float, float, float, float]]
    ) -> None:
        """
        Update the keyboard layout for the decoder.
        :param layout: A dictionary mapping keys to their (x, y, width, height) tuples
        """
        self.suffix_decoder.update_layout(layout)

    def set_context(self, context: str) -> None:
        """
        Set the context for the language model.
        :param context: The context string to set
        """
        self.context = context.strip()
        self.language_model.preprocess_sentence(self.context)

    def decode_word(
        self,
        top_n: int = 5,
    ) -> list[str]:
        gesture_candidates = self.suffix_decoder.decode(self.points)

        language_probs = {}
        # Predictions for contexts with 1 word or less are usually not useful
        context_idx = self.context.strip().count(" ")
        if self.context.strip():
            for candidate in gesture_candidates[:50]:
                self.language_model.add_word(candidate.word)

            predictions = self.language_model.predict_next_word(self.context)

            for candidate in gesture_candidates:
                word = candidate.word
                language_probs[word] = predictions.get(word, 0.0)

            for candidate in gesture_candidates[:10]:
                print(
                    f"Gesture: {candidate.word}, Prob: {candidate.probability}, Language Prob: {language_probs.get(candidate.word, 0.0)}, Gesture Distance: {candidate.gesture_distance}"
                )
            # for candidate in gesture_candidates[:50]:
            #     self.language_model.remove_word(candidate.word)

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
                    c.probability,
                    language_probs.get(c.word, 0.0),
                    context_idx,
                ),
                gesture_distance=getattr(c, "distance", 0.0),
            )
            for c in gesture_candidates
        ]

        integrated_scores.sort(key=lambda x: x.combined_probability, reverse=True)
        return [score.word for score in integrated_scores[:top_n]]

    def extract_sentence_context(self, word_data: WordData) -> str:
        words = word_data.target_sentence.split()
        if word_data.word_index <= 1:
            return ""

        return "".join(words[: word_data.word_index - 1])
