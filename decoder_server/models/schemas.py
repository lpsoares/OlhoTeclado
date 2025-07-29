from enum import Enum
from typing import List

from pydantic import BaseModel


class ContextUpdate(BaseModel):
    context: str


class PointsSimplePost(BaseModel):
    points: List[tuple[float, float, float]]


class DecodeParams(BaseModel):
    max_cand: int = 5


class DecoderStatus(str, Enum):
    CREATED = "created"
    INITIALIZING = "initializing"
    READY = "ready"
    ERROR = "error"
