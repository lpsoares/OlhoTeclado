# cython: language_level=3
# distutils: language = c++
# distutils: sources = [decoder/cython/eyeswipeDecoder.cpp, decoder/cython/prefixTree.cpp, decoder/cython/utils.cpp]
# distutils: extra_compile_args=-std=c++17

from pathlib import Path

import cython
from libcpp.string cimport string
from libcpp.vector cimport vector
from libcpp.pair cimport pair
from libcpp.map cimport map

cimport decoder.cython.trie as ctrie
from decoder.cython.utils import filter_saccades

from keyboard import KeyboardLayout


ASSETS = Path(__file__).parent.parent.parent / 'assets'


cdef extern from "eyeswipeDecoder.hpp" namespace "eyeswipe":
    cdef cppclass CWordScore:
        string word
        double dtwDistance
        int occurrences

    cdef cppclass CDecoder:
        CDecoder() except +
        vector[CWordScore] decode(ctrie.CTrie *trie, map[char, pair[double, double]] keyCenters, vector[pair[double, double]] gesture) except +


class WordScore:
    def __init__(self, word, gesture_prob, gesture_distance, occurrences, total_gesture, total_occurrences):
        self.word = word
        self.gesture_distance = gesture_distance
        self.occurrences = occurrences
        self.gesture_probability = gesture_prob / total_gesture
        self.language_probability = occurrences / total_occurrences

    @property
    def probability(self):
        return 0.95 * self.gesture_probability + 0.05 * self.language_probability


cdef class EyeSwipeGestureDecoder:
    cdef CDecoder _decoder
    cdef object _trie
    cdef object keyboard
    cdef set exceptions
    cdef map[char, pair[double, double]] key_centers

    def __init__(self):
        self._decoder = CDecoder()
        self._trie = ctrie.Trie(ASSETS / 'words.txt')
        self._trie.freqs_from_file(ASSETS / 'wordfreq.tsv')
        self.keyboard = KeyboardLayout(ASSETS / 'keys.csv')
        self.exceptions = set(["we", "yes", "let", "us", "get"])

        for c in "abcdefghijklmnopqrstuvwxyz'":
            self.key_centers[c] = self.keyboard[c].center

    @property
    def trie(self):
        return self._trie

    def decode(self, gesture: list[tuple[float, float, float]], first_letter: Optional[str]=None, last_letter: Optional[str]=None, candidate_limit: Optional[int]=10) -> list[WordScore]:
        """
        Decodes a gesture into a list of words.
        :param gesture: The gesture to decode.
        :param first_letter: The first letter of the prefix.
        :param last_letter: The last letter of the prefix.
        :param candidate_limit: The maximum number of candidates to return.
        :return: A list of WordScore objects containing the decoded words and their scores.
        """
        if not gesture:
            return []

        gesture = [(x, y) for _, x, y in filter_saccades(gesture)]

        cdef ctrie.Trie trie = self.trie
        if first_letter is not None or last_letter is not None:
            trie = self.trie.copy_partial(first_letter, last_letter)

        cscores = self._decoder.decode(trie._trie, self.key_centers, gesture)

        raw_scores = [(
            score.word.decode('utf-8'),
            1 / (1 + score.dtwDistance),
            score.dtwDistance,
            score.occurrences,
        ) for score in cscores]
        if candidate_limit and candidate_limit > 0:
            raw_scores.sort(key=lambda x: x[2])
            raw_scores = raw_scores[:candidate_limit]

        total_gesture = sum(score[1] for score in raw_scores)
        total_occurrences = sum(score[3] for score in raw_scores)
        words = set(score[0] for score in raw_scores)

        candidates = [WordScore(*score, total_gesture, total_occurrences) for score in raw_scores]

        if not candidate_limit or candidate_limit <= 0 or not any(exception in words for exception in self.exceptions):
            candidates.sort(key=lambda x: x.probability, reverse=True)
        return candidates
