from pathlib import Path

import numpy as np
import torch
from tqdm import tqdm
from transformers import AutoModelForCausalLM, AutoTokenizer

ASSETS = Path(__file__).parent.parent / "assets"


with open(ASSETS / "words.txt", "r", encoding="utf-8") as f:
    WORDS = [line.strip() for line in f if line.strip()]

# Load model and tokenizer
model_name = "gpt2"  # You can change this to any causal LM model
tokenizer = AutoTokenizer.from_pretrained(model_name)
# GPT-2 has no pad_token by default
tokenizer.pad_token = tokenizer.eos_token
model = AutoModelForCausalLM.from_pretrained(model_name)
model.eval()


# Precompute token IDs for all candidate second words
second_word_ids = {
    w: tokenizer(w, add_special_tokens=False).input_ids[0] for w in WORDS
}

# Batch tokenize all first words
first_word_tokens = tokenizer(WORDS, return_tensors="pt", padding=True)

print("Computing bigram probabilities...")
with torch.no_grad():
    outputs = model(**first_word_tokens)
print("Model inference done.")

# Get logits at the last position of each first word
logits = outputs.logits  # shape: (batch, seq_len, vocab_size)
# Index the last token position for each word
last_token_indices = first_word_tokens.attention_mask.sum(dim=1) - 1
next_token_logits = logits[torch.arange(len(WORDS)), last_token_indices, :]

# Softmax over vocab for each first word
print("Computing softmax probabilities...")
probs = torch.softmax(next_token_logits, dim=-1)

# Build probability matrix
probabilities = []
for i, w1 in tqdm(enumerate(WORDS), total=len(WORDS)):
    probabilities.append([])
    for w2, token_id in second_word_ids.items():
        probabilities[i].append(probs[i, token_id].item())

print(f"Saving probabilities to file {ASSETS / 'bigram_probabilities.npy'}")
probabilities = np.array(probabilities)
total_per_row = probabilities.sum(axis=1)
probabilities_normalized = probabilities / total_per_row[:, None]
np.save(ASSETS / "bigram_probabilities.npy", probabilities_normalized)
print("Probabilities saved successfully.")
