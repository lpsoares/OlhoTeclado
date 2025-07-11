from libcpp.string cimport string
from libcpp.pair cimport pair


cdef extern from "prefixTree.hpp" namespace "trie":
    cdef cppclass CTrie:
        cppclass Iterator:
            Iterator &operator++() except +
            Iterator operator+(int) except +
            bint operator==(const Iterator &other) except +
            bint operator!=(const Iterator &other) except +
            pair[string, int] operator*() except +

        CTrie() except +
        void insert(const string &word, int count) except +
        bint contains(const string &word) except +
        int length() except +
        int getOccurrences(const string &word) except +
        Iterator begin() except +
        Iterator end() except +


cdef class Trie:
    cdef CTrie *_trie
