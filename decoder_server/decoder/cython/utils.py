from typing import Optional


def filter_saccades(gesture: list[tuple[float, float, float]], threshold: Optional[float]=45) -> list[tuple[float, float, float]]:
    """
    Cleans up the gesture data by possible points sampled from saccades.
    :param gesture: The gesture data to clean up.
    :param threshold: The threshold distance for filtering saccades.
                      Default is 45 pixels.
    :return: The cleaned-up gesture data.
    """
    if len(gesture) < 2:
        return gesture

    threshold_sq = threshold ** 2
    clean_gesture = [gesture[0]]
    for i in range(1, len(gesture)-1):
        pt0 = gesture[i-1][1:]
        pt1 = gesture[i][1:]
        pt2 = gesture[i+1][1:]
        prev_distance_sq = (pt0[0] - pt1[0]) ** 2 + (pt0[1] - pt1[1]) ** 2
        next_distance_sq = (pt1[0] - pt2[0]) ** 2 + (pt1[1] - pt2[1]) ** 2
        if prev_distance_sq > threshold_sq or next_distance_sq > threshold_sq:
            continue
        clean_gesture.append(gesture[i])
    pt1 = gesture[-1][1:]
    pt2 = gesture[-2][1:]
    if (pt1[0]-pt2[0])**2 + (pt1[1]-pt2[1])**2 < threshold_sq:
        clean_gesture.append(gesture[-1])
    return clean_gesture


def filter_fixations(gesture: list[tuple[float, float, float]], threshold: Optional[float]=45) -> list[tuple[float, float, float]]:
    """
    Group the gesture data into fixations.
    :param gesture: The gesture data to clean up.
    :param threshold: The threshold distance for filtering fixations.
                      Default is 45 pixels.
    :return: The cleaned-up gesture data.
    """
    if not gesture:
        return []

    threshold_sq = threshold ** 2

    fixations = []
    cur_fixation = [gesture[0]]
    for i in range(1, len(gesture)):
        pt0 = gesture[i-1][1:]
        pt1 = gesture[i][1:]
        distance_sq = (pt0[0] - pt1[0]) ** 2 + (pt0[1] - pt1[1]) ** 2
        if distance_sq < threshold_sq:
            cur_fixation.append(gesture[i])
        else:
            fixations.append(cur_fixation)
            cur_fixation = [gesture[i]]
    fixations = [fixation for fixation in fixations if len(fixation) > 1]
    if cur_fixation:
        fixations.append(cur_fixation)
    return [(fixation[0][0], *avg(fixation)) for fixation in fixations]


def avg(fixation: list[tuple[float, float, float]]) -> tuple[float, float]:
    """
    Calculate the average of a fixation.
    :param fixation: The fixation data to average.
    :return: The average of the fixation data.
    """
    x_sum = sum(pt[1] for pt in fixation)
    y_sum = sum(pt[2] for pt in fixation)
    return x_sum / len(fixation), y_sum / len(fixation)
