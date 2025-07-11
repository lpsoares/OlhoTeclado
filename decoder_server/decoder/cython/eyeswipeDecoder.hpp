#ifndef EYESWIPEDECODER_H
#define EYESWIPEDECODER_H

#include <map>
#include "prefixTree.hpp"

namespace eyeswipe
{
  struct CWordScore
  {
    std::string word;
    double dtwDistance;
    int occurrences;
  };

  class CDecoder
  {
  public:
    std::vector<CWordScore> decode(trie::CTrie *trie, std::map<char, std::pair<double, double>> keyCenters, std::vector<std::pair<double, double>> gesture);
  };
} // namespace eyeswipe

#endif // EYESWIPEDECODER_H
