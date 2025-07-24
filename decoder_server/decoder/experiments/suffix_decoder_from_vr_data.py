import sys
from pathlib import Path

from datareader import find_keyboard_config, iter_words, read_log_file

sys.path.append(".")
from decoder import SuffixGestureDecoder

DATA = Path(__file__).parent / ".." / ".." / ".." / "experiment_server" / "data"
SESSION_DIR = DATA / "TestParticipant" / "green" / "session-01"

for i in range(3, 8):
    log_file = SESSION_DIR / f"trial-00{i}.csv"

    log_data = read_log_file(log_file)
    keyboard_config = find_keyboard_config(log_data)

    decoder = SuffixGestureDecoder(is_api=True, keyboard_config=keyboard_config)

    for word, gaze_path in iter_words(log_data):
        candidates = decoder.decode(gaze_path)
        words = [c.word for c in candidates]
        try:
            index = words.index(word) if words else -1
        except ValueError:
            index = -1
        top = f"top-{index + 1}" if index >= 0 else "not found"
        title = f"{word} ({top})"
        print(title)
        # print("=" * len(title))
        # for c in candidates[:10]:
        #     print(f"{c.word}: {c.gesture_distance} -- {c.probability}")
        # print()

"""
Results as they were:
suburbs (top-1)
are (top-8)
sprawling (top-1)
up (top-1)
everywhere (top-1)
frequently (top-1)
asked (top-1)
questions (top-1)
a (top-16)
most (top-2)
ridiculous (top-1)
thing (top-1)
zero (top-1)
in (top-3)
on (top-3)
the (top-9)
facts (top-1)
the (top-3)
registration (top-1)
period (top-4910)
period (top-2)
is (top-41)
over (top-12)

After rescaling DEFAULT_KEY_THRESHOLD by key_size (2/3):
suburbs (top-1)
are (top-1)
sprawling (top-1)
up (top-1)
everywhere (top-1)
frequently (top-1)
asked (top-1)
questions (not found)
a (top-7)
most (top-1)
ridiculous (not found)
thing (not found)
zero (top-1)
in (top-3)
on (not found)
the (top-1)
facts (top-1)
the (top-1)
registration (top-1)
period (not found)
period (top-1)
is (top-16)
over (top-1)

After rescaling DEFAULT_KEY_THRESHOLD by key_size (1.2):
suburbs (top-1)
are (top-1)
sprawling (top-1)
up (top-1)
everywhere (top-1)
frequently (top-1)
asked (top-1)
questions (top-1)
a (top-7)
most (top-2)
ridiculous (top-1)
thing (top-1)
zero (top-1)
in (top-3)
on (top-3)
the (top-6)
facts (top-1)
the (top-1)
registration (top-1)
period (not found)
period (top-1)
is (top-16)
over (top-10)

After updating best distance only if current first point is close to the key (same distance as last letter: 1.2 * key_size):
suburbs (top-1)
are (top-1)
sprawling (top-1)
up (top-1)
everywhere (top-1)
frequently (top-1)
asked (top-1)
questions (top-1)
a (top-4)
most (top-2)
ridiculous (top-1)
thing (top-1)
zero (top-1)
in (top-572)
on (top-3)
the (top-6)
facts (top-2)
the (top-1)
registration (top-1)
period (not found)
period (top-1)
is (top-8)
over (top-6)

After updating best distance only if current first point is close to the key (same distance as last letter: 1.5 * key_size):
suburbs (top-1)
are (top-5)
sprawling (top-1)
up (top-1)
everywhere (top-1)
frequently (top-1)
asked (top-1)
questions (top-1)
a (top-6)
most (top-2)
ridiculous (top-1)
thing (top-1)
zero (top-1)
in (top-3)
on (top-3)
the (top-6)
facts (top-2)
the (top-1)
registration (top-1)
period (not found)
period (top-1)
is (top-14)
over (top-8)
"""
