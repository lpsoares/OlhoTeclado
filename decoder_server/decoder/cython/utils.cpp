#include <cmath>
#include "utils.hpp"

namespace utils
{
  double distance(const std::pair<double, double> &a, const std::pair<double, double> &b)
  {
    return sqrt((a.first - b.first) * (a.first - b.first) + (a.second - b.second) * (a.second - b.second));
  }

  double **createMatrix(std::size_t rows, std::size_t cols)
  {
    double **matrix = new double *[rows];
    for (std::size_t i = 0; i < rows; i++)
    {
      matrix[i] = new double[cols];
      for (std::size_t j = 0; j < cols; j++)
      {
        matrix[i][j] = 0;
      }
    }
    return matrix;
  }

  void deleteMatrix(double **matrix, std::size_t rows)
  {
    for (std::size_t i = 0; i < rows; i++)
    {
      delete[] matrix[i];
    }
    delete[] matrix;
  }

  double normalPDF(double x, double mean, double stddev)
  {
    return (1.0 / (stddev * sqrt(2.0 * M_PI))) * exp(-0.5 * ((x - mean) * (x - mean)) / (stddev * stddev));
  }

  double normalCDF(double x, double mean, double stddev)
  {
    return 0.5 * (1.0 + erf((x - mean) / (stddev * sqrt(2.0))));
  }
}
