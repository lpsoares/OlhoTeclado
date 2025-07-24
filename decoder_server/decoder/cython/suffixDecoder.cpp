#include <cmath>
#include <string>
#include <vector>
#include <iterator>
#include <cstddef>
#include "suffixDecoder.hpp"
#include "utils.hpp"
#include <limits>

using namespace utils;

namespace suffixDecoder
{
  double min(double a, double b);
  double min(double a, double b, double c);
  double max(double a, double b);
  double max(double a, double b, double c);

  // This function assumes that the input is normalized by the key size (i.e., the diameter of the key is 1).
  std::vector<CWordScore> CDecoder::decode(trie::CTrie *trie, std::map<char, std::pair<double, double>> keyCenters, std::vector<std::pair<double, double>> gesture, std::string lastLetters, double keyDistThresh)
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
          double cost = max(distance(pt1, pt2) - 0.5, 0);

          double matchCost = dtw[curWord.length() - 1][i - 1];
          double insertionCost = dtw[curWord.length()][i - 1];
          double deletionCost = dtw[curWord.length() - 1][i];

          double opCost = min(matchCost, insertionCost, deletionCost);
          dtw[curWord.length()][i] = cost + opCost;
        }
        if (node->isEndOfWord())
        {
          double bestDistance = std::numeric_limits<double>::max();
          for (std::size_t i = 0; i <= gesture.size(); i++)
          {
            double newDistance = dtw[curWord.length()][i] / ((i + 1) * (i + 1));
            // We only consider the word if the last (first - since it's reversed) gesture is close to the last letter of the word
            if (newDistance < bestDistance && distance(gesture[i - 1], keyCenters[curWord.back()]) < keyDistThresh)
            {
              bestDistance = newDistance;
            }
          }

          scores.at(scoreIdx)
              .word = curWord;
          scores.at(scoreIdx).dtwBestDistance = bestDistance;
          scoreIdx++;
        }
      }
      for (const auto &child : node->children)
      {
        if (curWord.length() > 0 || lastLetters.find(child.first) != std::string::npos)
        {
          stack.push({child.second, curWord + child.first});
        }
      }
    }
    deleteMatrix(dtw, trie->getMaxWordLength() + 1);
    return scores;
  }

  double min(double a, double b)
  {
    return (a < b) ? a : b;
  }

  double min(double a, double b, double c)
  {
    return min(min(a, b), c);
  }

  double max(double a, double b)
  {
    return (a > b) ? a : b;
  }

  double max(double a, double b, double c)
  {
    return max(max(a, b), c);
  }

  double distance(const std::pair<double, double> &a, const std::pair<double, double> &b)
  {
    return std::sqrt((a.first - b.first) * (a.first - b.first) + (a.second - b.second) * (a.second - b.second));
  }
} // namespace suffixDecoder
