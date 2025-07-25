from functools import lru_cache
from typing import Dict, List, Optional

import torch
from transformers import AutoModelForCausalLM, AutoTokenizer


class GPT2LanguageModel:
    def __init__(self, words: Optional[List[str]] = None, device: str = "auto"):
        if device == "auto":
            if torch.cuda.is_available():
                self.device = "cuda"
            else:
                self.device = "cpu"
        else:
            self.device = device

        self.model_name = "gpt2"

        self.model = AutoModelForCausalLM.from_pretrained(
            self.model_name,
            torch_dtype=torch.float16 if self.device != "cpu" else torch.float32,
            device_map=self.device if self.device != "cpu" else None,
        ).eval()

        self.tokenizer = AutoTokenizer.from_pretrained(self.model_name)

        self.first_words_to_token_ids = {}
        self.non_first_words_to_token_ids = {}
        self.token_ids_to_words = {}

        self._tokenize_cache = {}

        self.softmax = torch.nn.functional.softmax

        if words:
            self._batch_add_words(words)

    def _batch_add_words(self, words: List[str]):
        """batch add words to the model's vocab"""
        for word in words:
            self.add_word(word)

    def set_words(self, words: List[str]):
        """Set the words for the model, clearing previous words"""
        self.first_words_to_token_ids.clear()
        self.non_first_words_to_token_ids.clear()
        self.token_ids_to_words.clear()
        self._batch_add_words(words)

    def add_word(self, word: str):
        """Add a word to the models vocab"""
        if word in self.first_words_to_token_ids:
            return

        token_id_first = self._tokenize_cached(word, as_first_word=True)[0]
        self.first_words_to_token_ids[word] = token_id_first
        self.token_ids_to_words.setdefault(token_id_first, []).append(word)

        token_id_non_first = self._tokenize_cached(word, as_first_word=False)[0]
        self.non_first_words_to_token_ids[word] = token_id_non_first
        self.token_ids_to_words.setdefault(token_id_non_first, []).append(word)

    @lru_cache(maxsize=10000)
    def _tokenize_cached(self, word: str, as_first_word: bool = True) -> tuple:
        """caches tokenization for most used words"""
        word = word.strip()
        if not as_first_word:
            word = f" {word}"
        tokens = self.tokenizer.encode(word, return_tensors="pt")[0].tolist()
        return tuple(tokens)

    def predict_next_word(self, sentence: str) -> Dict[str, float]:
        """Predicts the next word probabilities based on a given sentence"""
        if sentence:
            token_ids = list(self.non_first_words_to_token_ids.values())
        else:
            token_ids = list(self.first_words_to_token_ids.values())

        if not token_ids:
            return {}

        predictions = self._get_next_token_probabilities(sentence, token_ids)

        result = {}
        for token_id, probability in predictions.items():
            words_for_token = self.token_ids_to_words.get(token_id, [])
            for word in words_for_token:
                result[word] = probability

        return result

    @lru_cache(maxsize=1000)
    def _get_sentence_encoding(self, sentence: str) -> torch.Tensor:
        sentence = sentence.strip() + " "  # Ensure there's a space at the end
        return self.tokenizer.encode(sentence, return_tensors="pt").to(self.device)

    def _get_next_token_probabilities(
        self, sentence: str, token_ids: List[int]
    ) -> Dict[int, float]:
        """gets the next token probabilities for a given sentence and token IDs"""
        inputs = self._get_sentence_encoding(sentence)

        token_ids_tensor = torch.tensor(token_ids, device=self.device, dtype=torch.long)

        with torch.no_grad():
            with torch.inference_mode():
                outputs = self.model(inputs)
                last_token_logits = outputs.logits[0, -1, :]

                relevant_logits = last_token_logits[token_ids_tensor]
                probabilities = self.softmax(relevant_logits, dim=0)

                return dict(zip(token_ids, probabilities.cpu().numpy().tolist()))

    def clear_cache(self):
        self._tokenize_cached.cache_clear()
        self._get_sentence_encoding.cache_clear()
        self._get_sentence_encoding.cache_clear()
