import csv


def read_log_file(file_path):
    events = []
    with open(file_path, "r", encoding="utf-8") as f:
        reader = csv.reader(f)
        for i, row in enumerate(reader):
            if i == 0:
                continue
            tstamp, data_type, raw_data = row
            event = EVENT_PARSERS[data_type](raw_data)
            event["type"] = data_type
            event["timestamp"] = float(tstamp)
            events.append(event)
    return events


def parse_trial_start(raw_data: str) -> dict[str, str]:
    return {
        "target_text": raw_data,
    }


def parse_trial_end(raw_data: str) -> dict[str, str]:
    return {"final_text": raw_data}


def parse_key_press(raw_data: str) -> dict[str, str]:
    name, label, value = raw_data.split(";")
    return {
        "name": name,
        "label": label,
        "value": value,
    }


def parse_key_pos(raw_data: str) -> dict[str, dict[str, dict[str, str | float]]]:
    parts = raw_data.split(";")
    pos_data = {}
    for i in range(0, len(parts), 7):
        context = parts[i + 2]
        key_name = parts[i]
        pos_data.setdefault(context, {})[key_name] = {
            "key_name": key_name,
            "key_label": parts[i + 1],
            "context": context,
            "width": float(parts[i + 3]),
            "height": float(parts[i + 4]),
            "x_2d": float(parts[i + 5]),
            "y_2d": float(parts[i + 6]),
        }
    return pos_data


def parse_ctx_pos(raw_data: str) -> dict[str, dict[str, str | float]]:
    parts = raw_data.split(";")
    ctx_data = {}
    for i in range(0, len(parts), 13):
        context_name = parts[i]
        ctx_data[context_name] = {
            "context_name": context_name,
            "x_3d_origin": float(parts[i + 1]),
            "y_3d_origin": float(parts[i + 2]),
            "z_3d_origin": float(parts[i + 3]),
            "x_3d_up": float(parts[i + 4]),
            "y_3d_up": float(parts[i + 5]),
            "z_3d_up": float(parts[i + 6]),
            "x_3d_right": float(parts[i + 7]),
            "y_3d_right": float(parts[i + 8]),
            "z_3d_right": float(parts[i + 9]),
            "x_3d_forward": float(parts[i + 10]),
            "y_3d_forward": float(parts[i + 11]),
            "z_3d_forward": float(parts[i + 12]),
        }
    return ctx_data


def parse_context_change(raw_data: str) -> dict[str, str]:
    original_context, new_context = raw_data.split(";")
    return {
        "original_context": original_context,
        "new_context": new_context,
    }


def parse_text_change(raw_data: str) -> dict[str, str]:
    return {
        "new_text": raw_data,
    }


def parse_candidates(raw_data: str) -> dict[str, list[str]]:
    candidates = raw_data.split(";")
    return {
        "candidates": candidates,
    }


def parse_gaze_data(raw_data: str) -> dict[str, float]:
    parts = raw_data.split(";")
    return {
        "x_2d": float(parts[0]),
        "y_2d": float(parts[1]),
        "x_3d": float(parts[2]),
        "y_3d": float(parts[3]),
        "z_3d": float(parts[4]),
        "left_eye_x_3d": float(parts[5]),
        "left_eye_y_3d": float(parts[6]),
        "left_eye_z_3d": float(parts[7]),
        "right_eye_x_3d": float(parts[8]),
        "right_eye_y_3d": float(parts[9]),
        "right_eye_z_3d": float(parts[10]),
        "left_direction_x_3d": float(parts[11]),
        "left_direction_y_3d": float(parts[12]),
        "left_direction_z_3d": float(parts[13]),
        "right_direction_x_3d": float(parts[14]),
        "right_direction_y_3d": float(parts[15]),
        "right_direction_z_3d": float(parts[16]),
    }


EVENT_PARSERS = {
    "TRIAL_START": parse_trial_start,
    "TRIAL_END": parse_trial_end,
    "KEY_PRESS": parse_key_press,
    "KEY_POS": parse_key_pos,
    "CTX_POS": parse_ctx_pos,
    "CONTEXT_CHANGE": parse_context_change,
    "TEXT_CHANGE": parse_text_change,
    "CANDIDATES": parse_candidates,
    "GAZE": parse_gaze_data,
}


def find_keyboard_config(log_data) -> dict[str, tuple[float, float, float, float]]:
    keyboard_config = {}
    for event in log_data:
        if event["type"] == "KEY_POS":
            context_data = event.get("Current")
            if context_data:
                for key in context_data.values():
                    if key["key_name"].startswith("Key_"):
                        key_name = key["key_name"].split("_")[-1].lower()
                        keyboard_config[f"{key_name}Key"] = [
                            key["x_2d"],
                            key["y_2d"],
                            key["width"],
                            key["height"],
                        ]
                break
    return keyboard_config


def iter_words(log_data):
    target_sentence = None
    current_text = ""
    current_word = None
    is_current_context = True
    gaze_path = []
    for event in log_data:
        if event["type"] == "TRIAL_START":
            target_sentence = event["target_text"].lower().replace(".", "")
            current_word = target_sentence.split()[0]
        elif event["type"] == "TEXT_CHANGE":
            prev_text = current_text
            current_text = event["new_text"].strip().replace(".", "")
            # If equals, it was a candidate selection, if greater, it was a deletion
            if len(prev_text.split()) < len(current_text.split()):
                yield current_word, gaze_path
            gaze_path = []

            # current_word is the first word that is in the target sentence but not in current_text
            if not target_sentence:
                raise ValueError("Target sentence not set.")
            if not current_text:
                raise ValueError("Current text not set.")
            target_words = target_sentence.split()
            current_words = current_text.split()

            current_word = None
            for target, current in zip(target_words, current_words):
                if target != current:
                    current_word = target
                    break
            if current_word is None and len(current_words) < len(target_words):
                current_word = target_words[len(current_words)]
        elif event["type"] == "CONTEXT_CHANGE":
            is_current_context = event["new_context"] == "Current"
        elif event["type"] == "GAZE" and is_current_context:
            gaze_path.append((event["timestamp"], event["x_2d"], event["y_2d"]))
