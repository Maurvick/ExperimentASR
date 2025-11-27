import whisper
from whisper import transcribe
import json

def transcribe_whisper(model_size, file_path, lang="en"):
    print(json.dumps({
    "status": "info",
    "message": f'Initiating {file_path} transcription...'
    }))
    model = whisper.load_model(model_size)
    result = model.transcribe(
        file_path,
        language=lang,          # None = auto-detect
        task="transcribe",      # or "translate" to English
        temperature=0.0,
        best_of=5,
        beam_size=5,
        fp16=True  # Set True if you have GPU
    )
    return result["text"]
