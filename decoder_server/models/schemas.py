from pydantic import BaseModel
from typing import List
from enum import Enum

class ContextUpdate(BaseModel):
    context: str

class PointsSimplePost(BaseModel):
    points: List[tuple[float, float, float]] 

class DecoderStatus(str, Enum):
    INITIALIZING = "initializing"
    READY = "ready"
    ERROR = "error"