#ifndef PREFIX_TREE_H
#define PREFIX_TREE_H

#include <map>
#include <stack>

namespace trie
{
  struct TrieNode
  {
    std::map<char, TrieNode *> children;
    int count;

    TrieNode();
    ~TrieNode();
    TrieNode *getChild(char ch);
    bool isEndOfWord();
  };

  class CTrie
  {
  public:
    struct Iterator
    {
      using iterator_category = std::input_iterator_tag;
      using difference_type = std::ptrdiff_t;
      using value_type = std::pair<std::string, int>;
      using pointer = value_type *;
      using reference = value_type &;

      Iterator();
      Iterator(TrieNode *root);
      Iterator &operator++();
      Iterator operator+(std::size_t i);
      bool operator==(const Iterator &other) const;
      bool operator!=(const Iterator &other) const;
      std::pair<std::string, int> operator*();
      std::pair<std::string, int> *operator->();
      void reset();

    private:
      TrieNode *root;
      std::stack<std::pair<TrieNode *, std::string>> stack;
      std::pair<std::string, int> current;

      void advance();
    };

    CTrie();
    ~CTrie();
    void insert(const std::string &word, int count = -1);
    bool contains(const std::string &word);
    int length();
    int getOccurrences(const std::string &word);
    Iterator begin();
    Iterator end();
    int getMaxWordLength() const;
    TrieNode *getRoot();

  private:
    TrieNode *root;
    int totalWords;
    std::size_t maxWordLength;

    TrieNode *getNode(const std::string &word, bool create = false);
  };
}

#endif // PREFIX_TREE_H
