import soundfile as sf
import os
import numpy as np

# —творюЇмо папку дл€ файл≥в
os.makedirs("extracted_audio", exist_ok=True)

# ≤теруЇмо по датасет≥ та збер≥гаЇмо ауд≥о
for i, example in enumerate(dataset["train"]):  # або dataset, €кщо без спл≥т≥в
    audio_array = np.array(example["audio"]["array"])  # numpy-масив
    sample_rate = example["audio"]["sampling_rate"]
    
    file_path = f"extracted_audio/audio_{i:04d}.wav"
    sf.write(file_path, audio_array, sample_rate)
    
    # ќпц≥онально: збер≥гаЇмо транскрипц≥ю в TXT
    with open(f"extracted_audio/audio_{i:04d}.txt", "w", encoding="utf-8") as f:
        f.write(example["transcription"])
    
    print(f"«бережено: {file_path} (тривал≥сть: {example['duration']} с)")

# якщо весь датасет великий, обмежте: list(dataset["train"].select(range(50)))