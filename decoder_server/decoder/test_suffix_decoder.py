import pytest

from decoder import SuffixGestureDecoder
from iterdata import get_gesture, iterwords


decoder = SuffixGestureDecoder()


@pytest.mark.parametrize(
    'participant, session, word, start, stop',
    [
        pytest.param(
            participant, session, word, start, stop,
            id=f'P{participant} (session {session}) - "{word}" ({start:.2f}, {stop:.2f})'
        ) for participant, session, _, _, _, _, word, start, stop, *_
        in iterwords()
    ]
)
def test_pass(participant, session, word, start, stop):
    gesture = get_gesture(participant, session, start, stop)
    result = decoder.decode(gesture)

    top_10_candidates = [word_score.word for word_score in result[:10]]

    assert word in top_10_candidates, f"The word '{word}' was not found in the candidates. Top-10: {top_10_candidates}."
