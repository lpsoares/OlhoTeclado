from decoder import Trie


def test_trie_with_some_words():
    words = set(['hello', 'house', 'home', 'hope', 'homerun', 'houses', 'hell'])
    words_not_in_trie = set(['homer', 'ho', 'h', 'hoop', 'homes'])
    trie = Trie()
    for word in words:
        trie.insert(word)

    for word in words:
        assert word in trie
    for word in words_not_in_trie:
        assert word not in trie
    assert len(trie) == len(words)
