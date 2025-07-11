# cython: language_level=3
# distutils: language = c++
# distutils: sources = decoder/cython/prefixTree.cpp
# distutils: extra_compile_args=-std=c++17

from pathlib import Path

import cython
from libcpp.string cimport string
from libcpp.vector cimport vector
from libcpp.pair cimport pair
from libcpp.map cimport map

from keyboard import KeyboardLayout


ASSETS = Path(__file__).parent.parent.parent / 'assets'


cdef class Trie:
    def __init__(self, words_file = None):
        self._trie = new CTrie()
        if words_file:
            if not Path(words_file).exists():
                raise FileNotFoundError(f"File {words_file} not found.")
            self.words_from_file(words_file)

    def __dealloc__(self):
        del self._trie

    def insert(self, word: str, count: Optional[int] = None) -> None:
        if count is None:
            count = -1
        cdef string c_word = word.encode('utf-8')
        self._trie.insert(c_word, count)

    def __len__(self) -> int:
        """
        Returns the number of words in the trie.
        :return: The number of words in the trie.
        """
        return self._trie.length()

    def __contains__(self, word: str) -> bool:
        cdef string c_word = word.encode('utf-8')
        return self._trie.contains(c_word)

    def __iter__(self):
        """
        Returns an iterator over the words in the trie.
        :return: An iterator over the words in the trie.
        """
        cdef CTrie.Iterator it = self._trie.begin()
        cdef CTrie.Iterator end_it = self._trie.end()
        while it != end_it:
            word, count = cython.operator.dereference(it)
            yield word.decode('utf-8'), count
            it = it + 1

    def get_occurrences(self, word: str):
        """
        Returns the number of occurrences of a word in the trie.
        :param word: The word to search for.
        :return: The number of occurrences of the word in the trie.
        """
        return self._trie.getOccurrences(word)

    def words_from_file(self, word_file: Path) -> None:
        """
        Inserts words from a file into the trie.
        :param word_file: The path to the file containing words.
        """
        with open(word_file, 'r', encoding='utf-8') as f:
            for line in f:
                self.insert(line.strip())

    def freqs_from_file(self, freqs_file: Path, delimiter: str = None, create_if_not_exists: bool = False) -> None:
        """
        Inserts frequencies from a file into the trie.
        :param freqs_file: The path to the file containing frequencies.
        :param delimiter: The delimiter used in the file.
        :param create_if_not_exists: Whether to create the trie if it doesn't exist.
        """
        freqs_file = Path(freqs_file)
        if delimiter is None:
            if freqs_file.suffix == '.csv':
                delimiter = ','
            elif freqs_file.suffix == '.tsv':
                delimiter = '\t'
            else:
                delimiter = ' '
        with open(freqs_file, 'r', encoding='utf-8') as f:
            for line in f:
                word, freq = line.strip().split(delimiter)
                word = word.replace('.', '').replace('_', '').replace('“', "'").replace('”', "'")
                word = word.replace('‘', "'").replace('’', "'").replace('`', "'")
                word = word.lower()
                if not all(ord('a') <= ord(c) <= ord('z') or c == "'" for c in word):
                    continue
                if word in self:
                    self.insert(word, int(freq))
                elif create_if_not_exists:
                    self.insert(word, int(freq))

    def copy_partial(self, first_letter: Optional[str] = None, last_letter: Optional[str] = None) -> Trie:
        """
        Creates a copy of the trie with only the words that start with the given prefix.
        :param first_letter: The first letter(s) of the prefix.
        :param last_letter: The last letter(s) of the prefix.
        :return: A new Trie object containing the copied words.
        """
        new_trie = Trie()
        for word, count in self:
            if (first_letter is None or word[0] in first_letter) and (last_letter is None or word[-1] in last_letter):
                new_trie.insert(word, count)
        return new_trie
