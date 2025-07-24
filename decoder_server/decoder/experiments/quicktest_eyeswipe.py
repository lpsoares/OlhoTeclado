import sys

sys.path.append(".")
from decoder import EyeSwipeGestureDecoder

decoder = EyeSwipeGestureDecoder()
trie = decoder.trie

copied = trie.copy_partial("h", "s")
assert "home" in trie
assert "home" not in copied
assert "houses" in trie
assert "houses" in copied
print("Quicktest passed!")

print(f"Words in tree: {len(trie)}")
print(f"Words in partial tree: {len(copied)}")
