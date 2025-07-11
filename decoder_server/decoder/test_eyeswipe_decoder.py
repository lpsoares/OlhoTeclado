import pytest

from decoder import EyeSwipeGestureDecoder
from iterdata import get_gesture, iterwords


decoder = EyeSwipeGestureDecoder()


@pytest.mark.parametrize(
    'participant, session, word, start, stop, expected_candidates',
    [
        pytest.param(
            participant, session, word, start, stop, candidates,
            id=f'P{participant} (session {session}) - "{word}" ({start:.2f}, {stop:.2f})'
        ) for participant, session, _, _, _, _, word, start, stop, _, candidates, _
        in iterwords()
    ]
)
def test_pass(participant, session, word, start, stop, expected_candidates):
    first_letter = word[0]
    last_letter = word[-1]
    gesture = get_gesture(participant, session, start, stop)
    result = decoder.decode(gesture, first_letter, last_letter)
    candidates = set([word_score.word for word_score in result][:5])
    expected_candidates = set(expected_candidates)
    # Check if the candidates contain at most one incorrect word
    assert len(candidates.intersection(expected_candidates)
               ) >= len(expected_candidates) - 1
    if word in expected_candidates:
        # Check if the correct word is in the candidates
        assert word in candidates
