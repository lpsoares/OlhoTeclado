#ifndef SUFFIXDECODER_H
#define SUFFIXDECODER_H

#include <map>
#include "prefixTree.hpp"

namespace suffixDecoder
{
  struct CWordScore
  {
    std::string word;
    double dtwBestDistance;
  };

  class CDecoder
  {
  public:
    std::vector<CWordScore> decode(trie::CTrie *trie, std::map<char, std::pair<double, double>> keyCenters, std::vector<std::pair<double, double>> gesture, std::string lastLetters);
  };
} // namespace suffixDecoder

#endif // SUFFIXDECODER_H
