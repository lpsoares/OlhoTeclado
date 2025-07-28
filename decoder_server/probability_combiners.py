from typing import Callable

def linear_combiner(
    alpha1: float,
    alpha2: float,
    x: float,
) -> Callable[[float, float], float]:
    """
    creates a linear combination function that scales the language probability with an exponential factor 
    
    Args:
        alpha1: weight for the gesture probability
        alpha2: weight for the language probability
        x: exponent to scale the language probability

    Returns:
        A function that takes two probabilities (gesture and language) and returns their combined probability
    """

    def combine(gesture_prob: float, language_prob: float, context_idx: int) -> float:
        if context_idx == 0:
            return gesture_prob
        
        context_weight = min(context_idx / 10.0, 1.0)
        effective_alpha2 = alpha2 * context_weight
        
        return alpha1 * gesture_prob + effective_alpha2 * (language_prob ** x)
    return combine
