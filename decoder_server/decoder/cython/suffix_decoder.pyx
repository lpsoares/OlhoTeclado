# cython: language_level=3
# distutils: language = c++
# distutils: sources = [decoder/cython/suffixDecoder.cpp, decoder/cython/prefixTree.cpp, decoder/cython/utils.cpp]
# distutils: extra_compile_args=-std=c++17

from pathlib import Path

import cython
from libcpp.string cimport string
from libcpp.vector cimport vector
from libcpp.pair cimport pair
from libcpp.map cimport map

cimport decoder.cython.trie as ctrie
from decoder.cython.utils import filter_saccades, filter_fixations

from keyboard import KeyboardLayout


ASSETS = Path(__file__).parent.parent.parent / 'assets'
DEFAULT_KEY_THRESHOLD = 60


cdef extern from "suffixDecoder.hpp" namespace "suffixDecoder":
    cdef cppclass CWordScore:
        string word
        double dtwBestDistance

    cdef cppclass CDecoder:
        CDecoder() except +
        vector[CWordScore] decode(ctrie.CTrie *trie, map[char, pair[double, double]] keyCenters, vector[pair[double, double]] gesture, string lastLetters) except +


class WordScore:
    def __init__(self, word, gesture_prob, gesture_distance, total_gesture):
        self.word = word
        self.gesture_distance = gesture_distance
        self.probability = gesture_prob / total_gesture


cdef class SuffixGestureDecoder:
    cdef CDecoder _decoder
    cdef object _trie
    cdef object keyboard
    cdef map[char, pair[double, double]] key_centers

    def __init__(self, is_api: bool = False, keyboard_config: dict[str, tuple[float, float, float, float]] = None):
        self._decoder = CDecoder()
        self._trie = ctrie.Trie()
        if is_api and keyboard_config:
            self.keyboard = KeyboardLayout()
            self.keyboard.from_keyboard_config(keyboard_config)
        else:  
            self.keyboard = KeyboardLayout(ASSETS / 'keys.csv')

        with open(ASSETS / 'words.txt', 'r', encoding='utf-8') as f:
            for line in f:
                self._trie.insert(line.strip()[::-1])

        for c in "abcdefghijklmnopqrstuvwxyz'":
            self.key_centers[c] = self.keyboard[c].normalized_center

    @property
    def trie(self):
        return self._trie

    def decode(self, gesture: list[tuple[float, float, float]]) -> list[WordScore]:
        """
        Decodes a gesture into a list of words.
        :param gesture: The gesture to decode.
        :return: A list of WordScore objects containing the decoded words and their scores.
        """
        if not gesture:
            return []

        key_size = self.keyboard.key_size
        reversed_gesture = [(x, y) for _, x, y in filter_fixations(gesture, threshold=key_size/2)][::-1]
        reversed_gesture = [(x / key_size, y / key_size) for x, y in reversed_gesture]

        cdef ctrie.Trie trie = self.trie
        last_letters = self._get_last_letter_candidates(gesture, 200)

        cscores = self._decoder.decode(trie._trie, self.key_centers, reversed_gesture, ''.join(last_letters).encode('utf-8'))

        raw_scores = [(
            score.word.decode('utf-8')[::-1],
            1 / (1 + score.dtwBestDistance),
            score.dtwBestDistance,
        ) for score in cscores]
        # Will return a lot of empty strings because some words haven't been processed because they didn't end with the expected letters
        raw_scores = [score for score in raw_scores if score[0]]

        total_gesture = sum(score[1] for score in raw_scores)
        candidates = [WordScore(*score, total_gesture) for score in raw_scores]

        candidates.sort(key=lambda x: x.probability, reverse=True)
        return candidates

    def _get_last_letter_candidates(self, gesture: list[tuple[float, float, float]], time_threshold: float) -> list[str]:
        """
        Get the last letter candidates from the last time_threshold ms of the gesture.
        :param gesture: The gesture to decode.
        :param time_threshold: The time threshold in ms.
        :return: A list of last letter candidates.
        """
        if not gesture:
            return []
        last_tstamp = gesture[-1][0]
        keys = set()
        for i in range(len(gesture) - 1, -1, -1):
            if gesture[i][0] < last_tstamp - time_threshold:
                break
            candidates = self.keyboard.keys_close_to(gesture[i][1], gesture[i][2], DEFAULT_KEY_THRESHOLD)
            keys.update([c.key for c in candidates])
        if 'punct' in keys:
            keys.remove('punct')
        return list(keys)
