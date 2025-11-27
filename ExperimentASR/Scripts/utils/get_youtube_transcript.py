from youtube_transcript_api import YouTubeTranscriptApi, TranscriptsDisabled, NoTranscriptFound, VideoUnavailable
import re

def extract_video_id(url: str) -> str:
    patterns = [
        r"(?:v=|/)([0-9A-Za-z_-]{11}).*",
    ]
    for pattern in patterns:
        match = re.search(pattern, url)
        if match:
            return match.group(1)
    raise ValueError("Invalid URL format. Please check the correctness of the link.")


def get_transcript(video_id: str, lang: str = 'en') -> str:
    try:
        transcript = YouTubeTranscriptApi.get_transcript(video_id, languages=[lang])
        text = " ".join([entry['text'] for entry in transcript])
        text = re.sub(r'\s+', ' ', text).strip()
        return text
    except TranscriptsDisabled:
        return "[Error] Transcripts are disabled for this video."
    except NoTranscriptFound:
        return "[Error] Transcript for the specified language was not found."
    except VideoUnavailable:
        return "[Error] The video is unavailable or has been removed."
    except Exception as e:
        return f"[Unknown error] {e}"

