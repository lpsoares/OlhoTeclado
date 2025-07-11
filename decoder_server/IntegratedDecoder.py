import time
from dataclasses import dataclass
from typing import Callable

from decoder import SuffixGestureDecoder
from iterdata import WordData, get_gesture
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
    def __init__(self,
                gesture_weight: float = 0.8,
                language_weight: float = 0.2,
                combine_x: float = 0.8,
                combiner: Callable[[float, float], float] = linear_combiner,
                is_api: bool = False,
                keyboard_config: dict[str, tuple[float, float, float, float]] = None):
        self.suffix_decoder = SuffixGestureDecoder(is_api=is_api,
                                                    keyboard_config=keyboard_config)
        self.language_model = GPT2LanguageModel()
        self.combiner_fn = combiner(gesture_weight, language_weight, combine_x)
        self.gesture_weight = gesture_weight
        self.language_weight = language_weight
        self.combine_x = combine_x

    def decode_word(self, gesture_points: list[tuple[float, float, float]], context: str,top_n: int = 10) -> list[IntegratedWordScore]:
        
        gesture_candidates = self.suffix_decoder.decode(gesture_points)

        language_probs = {}
        if context.strip():
            start_time = time.time()
            predictions = self.language_model.predict_next_word(context)
            end_time = time.time()
            print(f"Language model prediction took {end_time - start_time:.4f} seconds for context: '{context}'")
            for candidate in gesture_candidates[:50]:
                self.language_model.add_word(candidate.word)
                
                for candidate in gesture_candidates:
                    word = candidate.word
                    language_probs[word] = predictions.get(word, 0.0)
        else:
            uniform_prob = 1.0/len(gesture_candidates) if gesture_candidates else 0
            language_probs = {candidate.word: uniform_prob for candidate in gesture_candidates}

        integrated_scores = [
            IntegratedWordScore(
                word=c.word,
                gesture_probability=c.probability,
                language_probability=language_probs.get(c.word, 0.0),
                combined_probability=self.combiner_fn(c.probability, language_probs.get(c.word, 0.0)),
                gesture_distance = getattr(c, "distance", 0.0)
            )
            for c in gesture_candidates
        ]

        integrated_scores.sort(key=lambda x: x.combined_probability, reverse = True)
        return integrated_scores[:top_n]
    

    def extract_sentence_context(self, word_data: WordData) -> str:
        words = word_data.target_sentence.split()
        if word_data.word_index <= 1:
            return ""
        
        return "".join(words[:word_data.word_index - 1])