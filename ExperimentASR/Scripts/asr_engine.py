import sys
import json
import jiwer
import numpy as np
from utils import google_transcribe, whisper_transcribe

def send_error(msg):
    print(json.dumps({"status": "error", "message": msg}))
    sys.exit(1)

if len(sys.argv) < 2:
    send_error("Usage: python YoutubeTranscriptCLI.py <URL> <lang>")


url = sys.argv[1]
lang = sys.argv[2]
asr_model= sys.argv[3] if len(sys.argv) > 3 else "whisper"
whisper_model = {
    0: "tiny",
    1: "base",
    2: "small",
    3: "medium",
    4: "large"
}

match asr_model:
    case "whisper":
        transcript = whisper_transcribe.transcribe_whisper(whisper_model, url, lang)
        print(json.dumps({
            "status": "ok",
            "transcript": transcript
        }))
    case "google":
        transcript = google_transcribe.transcribe_google(url)
        print(json.dumps({
            "status": "ok",
            "transcript": transcript
        }))
    case _:
        send_error(f"ASR model is not supported.")


def evaluate_wer_cer(reference_text, recognized_text):
    wer = jiwer.wer(reference_text, recognized_text)
    cer = jiwer.cer(reference_text, recognized_text)
    return wer, cer

