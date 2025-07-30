import uvicorn
import torch

if __name__ == "__main__":
    print(f"cuda available: {torch.cuda.is_available()}")
    uvicorn.run(
        "api.main:app",
        host="0.0.0.0",
        port=8000,
        workers=1,
    )