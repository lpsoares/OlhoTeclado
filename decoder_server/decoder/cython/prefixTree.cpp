#include <string>
#include "prefixTree.hpp"

namespace trie
{
  TrieNode::TrieNode() : count(0)
  {
  }

  TrieNode::~TrieNode()
  {
    for (auto &child : children)
    {
      delete child.second;
    }
    children.clear();
  }

  TrieNode *TrieNode::getChild(char ch)
  {
    auto it = children.find(ch);
    if (it != children.end())
    {
      return it->second;
    }
    return nullptr;
  }

  bool TrieNode::isEndOfWord()
  {
    return count > 0;
  }

  CTrie::Iterator::Iterator() : CTrie::Iterator::Iterator(nullptr)
  {
  }

  CTrie::Iterator::Iterator(TrieNode *root) : root(root)
  {
    reset();
  }

  CTrie::Iterator &CTrie::Iterator::operator++()
  {
    advance();
    return *this;
  }

  CTrie::Iterator CTrie::Iterator::operator+(std::size_t i)
  {
    for (std::size_t j = 0; j < i; ++j)
    {
      advance();
    }
    return *this;
  }

  bool CTrie::Iterator::operator==(const Iterator &other) const
  {
    if (this == &other)
      return true;
    if (current.first != other.current.first)
      return false;
    if (current.second != other.current.second)
      return false;
    return true;
  }

  bool CTrie::Iterator::operator!=(const Iterator &other) const
  {
    return !(*this == other);
  }

  std::pair<std::string, int> CTrie::Iterator::operator*()
  {
    return current;
  }

  std::pair<std::string, int> *CTrie::Iterator::operator->()
  {
    return &current;
  }

  void CTrie::Iterator::reset()
  {
    while (!stack.empty())
    {
      stack.pop();
    }
    if (root == nullptr)
    {
      current = {"", 0};
      return;
    }
    stack.push({root, ""});
    advance();
  }

  void CTrie::Iterator::advance()
  {
    while (!stack.empty())
    {
      auto [node, prefix] = stack.top();
      stack.pop();

      for (const auto &child : node->children)
      {
        stack.push({child.second, prefix + child.first});
      }

      if (node->isEndOfWord())
      {
        current = {prefix, node->count};
        return;
      }
    }
    current = {"", 0};
  }

  CTrie::CTrie() : totalWords(0), maxWordLength(0)
  {
    root = new TrieNode();
  }

  CTrie::~CTrie()
  {
    delete root;
    root = nullptr;
  }

  void CTrie::insert(const std::string &word, int count)
  {
    TrieNode *node = getNode(word, true);
    if (!node->isEndOfWord())
    {
      totalWords++;
    }
    node->count = count < 0 ? node->count + 1 : count;

    if (word.length() > maxWordLength)
    {
      maxWordLength = word.length();
    }
  }

  bool CTrie::contains(const std::string &word)
  {
    TrieNode *node = getNode(word);
    return node != nullptr && node->isEndOfWord();
  }

  int CTrie::length()
  {
    return totalWords;
  }

  int CTrie::getOccurrences(const std::string &word)
  {
    TrieNode *node = getNode(word);
    if (node != nullptr)
    {
      return node->count;
    }
    return 0;
  }

  CTrie::Iterator CTrie::begin()
  {
    return CTrie::Iterator(root);
  }

  CTrie::Iterator CTrie::end()
  {
    return CTrie::Iterator(nullptr);
  }

  int CTrie::getMaxWordLength() const
  {
    return maxWordLength;
  }

  TrieNode *CTrie::getRoot()
  {
    return root;
  }

  TrieNode *CTrie::getNode(const std::string &word, bool create)
  {
    TrieNode *node = root;
    for (char ch : word)
    {
      TrieNode *child = node->getChild(ch);
      if (child == nullptr)
      {
        if (create)
        {
          child = new TrieNode();
          node->children[ch] = child;
        }
        else
        {
          return nullptr;
        }
      }
      node = child;
    }
    return node;
  }
}
