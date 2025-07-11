#include <cmath>
#include <string>
#include <limits>
#include <vector>
#include <iterator>
#include <cstddef>
#include "eyeswipeDecoder.hpp"
#include "utils.hpp"

using namespace utils;

namespace eyeswipe
{
  std::vector<CWordScore> CDecoder::decode(trie::CTrie *trie, std::map<char, std::pair<double, double>> keyCenters, std::vector<std::pair<double, double>> gesture)
  {
    double **dtw = createMatrix(trie->getMaxWordLength() + 1, gesture.size() + 1);
    for (std::size_t i = 1; i <= gesture.size(); i++)
    {
      dtw[0][i] = std::numeric_limits<double>::max();
    }
    dtw[0][0] = 0;

    std::vector<CWordScore> scores(trie->length());
    int scoreIdx = 0;
    std::stack<std::pair<trie::TrieNode *, std::string>> stack;
    stack.push({trie->getRoot(), ""});
    while (!stack.empty())
    {
      auto [node, curWord] = stack.top();
      stack.pop();

      if (curWord.length())
      {
        dtw[curWord.length()][0] = std::numeric_limits<double>::max();

        std::pair<double, double> pt1 = keyCenters[curWord.at(curWord.length() - 1)];
        for (std::size_t i = 1; i <= gesture.size(); i++)
        {
          std::pair<double, double> pt2 = gesture[i - 1];
          double cost = distance(pt1, pt2);
          dtw[curWord.length()][i] = cost + std::min(
                                                dtw[curWord.length() - 1][i - 1], // match
                                                std::min(
                                                    dtw[curWord.length()][i - 1], // insertion
                                                    dtw[curWord.length() - 1][i]  // deletion
                                                    ));
        }
        if (node->isEndOfWord())
        {
          scores.at(scoreIdx).word = curWord;
          scores.at(scoreIdx).dtwDistance = dtw[curWord.length()][gesture.size()];
          scores.at(scoreIdx).occurrences = node->count;
          scoreIdx++;
        }
      }
      for (const auto &child : node->children)
      {
        stack.push({child.second, curWord + child.first});
      }
    }
    deleteMatrix(dtw, trie->getMaxWordLength() + 1);
    return scores;
  }
} // namespace eyeswipe
