from pathlib import Path

import numpy as np

ASSETS = Path(__file__).parent.parent / "assets"


with open(ASSETS / "words.txt", "r", encoding="utf-8") as f:
    WORDS = [line.strip() for line in f if line.strip()]
WORD_INDICES = {word: i for i, word in enumerate(WORDS)}


probabilities = np.load(ASSETS / "bigram_probabilities.npy", allow_pickle=True)
n = len(WORDS)


def test_probabilities_shape():
    assert probabilities.shape == (n, n), "Unexpected shape of probabilities array"


def test_total_one():
    totals = probabilities.sum(axis=1)
    assert np.allclose(totals, 1.0), "Probabilities for each first word do not sum to 1"


def test_some_probabilities():
    first_word = "the"
    word_probs = {
        WORDS[i]: probabilities[WORD_INDICES[first_word], i] for i in range(n)
    }
    sorted_pairs = sorted(word_probs.items(), key=lambda x: x[1], reverse=True)
    for i, (word, prob) in enumerate(sorted_pairs):
        print(i + 1, word, prob)
        if i == 100:
            break
    top_10_pairs = sorted_pairs[:10]
    top_10_words = [word for word, _ in top_10_pairs]
    assert "world" in top_10_words
