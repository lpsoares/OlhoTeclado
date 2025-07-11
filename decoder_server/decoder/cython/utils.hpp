#ifndef UTILS_H
#define UTILS_H

#include <utility>

namespace utils
{
  double distance(const std::pair<double, double> &a, const std::pair<double, double> &b);
  double **createMatrix(std::size_t rows, std::size_t cols);
  void deleteMatrix(double **matrix, std::size_t rows);
  double normalPDF(double x, double mean, double stddev);
  double normalCDF(double x, double mean, double stddev);
}

#endif // UTILS_H
