import sys
import glob
from setuptools import setup, Extension
from Cython.Build import cythonize

compiler_args = ["/std:c++17", "/D_USE_MATH_DEFINES"] if sys.platform == "win32" else ["-std=c++17"]

cpp_sources = glob.glob("decoder/cython/*.cpp")

cpp_sources = [file for file in cpp_sources if not file.endswith("_decoder.cpp") and not file.endswith(".pyx.cpp")]

extensions = [
    Extension(
        "decoder.cython.eyeswipe_decoder",
        ["decoder/cython/eyeswipe_decoder.pyx", "decoder/cython/eyeswipeDecoder.cpp", "decoder/cython/prefixTree.cpp", "decoder/cython/utils.cpp"],
        extra_compile_args=compiler_args,
        language="c++"
    ),
    Extension(
        "decoder.cython.suffix_decoder",
        ["decoder/cython/suffix_decoder.pyx", "decoder/cython/suffixDecoder.cpp", "decoder/cython/prefixTree.cpp", "decoder/cython/utils.cpp"],
        extra_compile_args=compiler_args,
        language="c++"
    ),
    Extension(
        "decoder.cython.trie",
        ["decoder/cython/trie.pyx", "decoder/cython/trie.cpp", "decoder/cython/prefixTree.cpp"],
        extra_compile_args=compiler_args,
        language="c++"
    )
]

setup(
    name='Swipe Decoder',
    ext_modules=cythonize(
        extensions,
        compiler_directives={'language_level': 3}
    ),
    py_modules=["decoder"],
)
