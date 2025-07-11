from pathlib import Path
import csv
from typing import Optional
from collections import namedtuple


WordData = namedtuple('WordData', ['participant', 'session', 'trial', 'target_sentence',
                      'trial_success', 'word_index', 'word',
                                   'start', 'stop', 'duration', 'candidates', 'candidate_changes'])


DATA = Path(__file__).parent.parent / 'data'
EXCLUDED_PARTICIPANTS = ['02', '11', 'andrew']


def iterwords(participant: Optional[str] = None, session: Optional[str] = None, skip_failed: Optional[bool] = False) -> list[WordData]:
    """
    Generates word data from the words.csv file.
    :param participant: The participant ID to filter by (optional)
    :param session: The session ID to filter by (optional)
    :param skip_failed: If True, skips trials that failed (optional)
    :return: A list of WordData tuples
    """
    all_trials_data = {}
    word_indices = {}
    data = []
    with open(DATA / 'words.csv') as f:
        word_data = csv.reader(f)
        for i, row in enumerate(word_data):
            if i == 0:
                continue  # skip header

            cur_participant, is_dwell, cur_session, word, start, stop, duration, deleted, *rest = row
            start, stop, duration = map(float, (start, stop, duration))
            is_dwell, deleted = eval(is_dwell), eval(deleted)

            if is_dwell or stop == 0:
                continue

            *candidates, candidate_changes = rest
            candidates = [c for c in candidates if c]
            candidate_changes = int(candidate_changes)

            if (
                (participant and cur_participant != participant) or
                (not participant and cur_participant in EXCLUDED_PARTICIPANTS) or
                (session and cur_session != session) or
                word == '0' or
                deleted
            ):
                continue

            if cur_session not in all_trials_data.setdefault(cur_participant, {}):
                all_trials_data[cur_participant][cur_session] = get_trial_data(
                    cur_participant, cur_session)
            trial_data = all_trials_data[cur_participant][cur_session]
            trial_index, trial_start, trial_end, trial_target_sentence, trial_typed_sentence = find_trial(
                trial_data, start)
            trial_success = trial_target_sentence == trial_typed_sentence
            word_index = word_indices.setdefault(cur_participant, {}).setdefault(
                cur_session, {}).get(trial_index, 0) + 1
            word_indices[cur_participant][cur_session][trial_index] = word_index

            if skip_failed and not trial_success:
                continue

            data.append(WordData(cur_participant, cur_session, trial_index, trial_target_sentence,
                        trial_success, word_index, word,
                        start, stop, duration, candidates, candidate_changes))
    return data


def get_trial_data(participant: str, session: str) -> list[tuple[float, float, str, str]]:
    """
    Returns the trial data for a given participant and session
    :param participant: The participant ID
    :param session: The session ID
    :return: A list of tuples containing the start and end timestamps, the target sentence and typed sentence
    """
    trials = []
    with open(DATA / participant / 'sessions' / 'gesture' / session / 'events.csv') as f:
        event_file = csv.reader(f)
        for row in event_file:
            if row[1] == 'EXP':
                start = float(row[0])
                target_sentence = ', '.join(row[2:])
            elif row[1] == 'TYP':
                end = float(row[0])
                typed_sentence = ', '.join(row[2:])

                if not start or not end or not target_sentence or not typed_sentence:
                    raise RuntimeError('Missing data')
                trials.append((start, end, target_sentence, typed_sentence))
    return trials


def find_trial(trial_data: list[tuple[float, float, str, str]], tstamp: float) -> tuple[float, float, str, str]:
    """
    Locate the trial that contains the given timestamp using binary search (we assume such trial always exists).
    """
    if trial_data[0][0] <= tstamp <= trial_data[0][1]:
        return 0, *trial_data[0]
    if trial_data[-1][0] <= tstamp <= trial_data[-1][1]:
        return len(trial_data)-1, *trial_data[-1]

    l = 0
    r = len(trial_data) - 1
    while r - l > 1:
        mid = (r + l) // 2
        trial = trial_data[mid]
        if trial[0] > tstamp:
            r = mid
        elif trial[1] < tstamp:
            l = mid
        else:
            return mid, *trial


def get_gesture(participant: str, session: str, start: float, stop: float):
    """
    Returns the gesture data for a given participant and session
    :param participant: The participant ID
    :param session: The session ID
    :param start: The start time of the gesture
    :param stop: The stop time of the gesture
    :return: A list of tuples containing the x and y coordinates of the gesture
    """
    gesture = []
    with open(DATA / participant / 'sessions' / 'gesture' / session / 'gaze.csv') as f:
        gesture_file = csv.reader(f)
        for row in gesture_file:
            tstamp, x, y = row
            tstamp = float(tstamp)
            if start <= tstamp <= stop:
                gesture.append((tstamp, float(x), float(y)))
    return gesture


if __name__ == '__main__':
    for word_data in iterwords():
        print(word_data)
