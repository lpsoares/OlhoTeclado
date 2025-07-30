from pathlib import Path

from language_model import GPT2LanguageModel

ASSETS = Path(__file__).parent.parent / "assets"
with open(ASSETS / "words.txt") as f:
    WORDS = f.read().splitlines()

MODEL = GPT2LanguageModel()


def test_some_sentences():
    sentences = [
        "my collection is complete",
        "the quick brown fox jumps over the lazy dog",
        "the cat sat on the mat",
        "the dog barked at the mailman",
        "the bird sang a sweet song",
    ]
    for sentence in sentences:
        words = sentence.split()
        for i in range(len(words) - 1):
            prefix = " ".join(words[: i + 1])
            next_word = words[i + 1]
            MODEL.set_words(words_limited_by(next_word[0], next_word[-1]))
            predictions = MODEL.predict_next_word(prefix)
            top_20 = sorted(predictions.items(), key=lambda x: x[1], reverse=True)[:20]
            print(f"Prefix: '{prefix}'")
            for word, probability in top_20:
                print(f"  {word}: {probability:.4f}")
            assert next_word in [word for word, _ in top_20], (
                f"Failed for prefix: '{prefix}'"
            )


def words_limited_by(start_char, ending_char):
    return [
        word
        for word in WORDS
        if word.startswith(start_char) and word.endswith(ending_char)
    ]
