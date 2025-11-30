import sys
import json
import argparse
import os
from utils import google_transcribe, whisper_transcribe

def send_error(msg):
    """Helper to print error as JSON and exit."""
    print(json.dumps({"status": "error", "message": msg}))
    sys.exit(1)


def main():
    # 1. Setup Argument Parser
    parser = argparse.ArgumentParser(description="YouTube Video Transcriber CLI")

    # Positional Argument: URL
    parser.add_argument("url", help="The YouTube URL to transcribe")

    # Optional Argument: Language
    parser.add_argument(
        "--lang", "-l", 
        default="en", 
        help="Language code (e.g., 'en', 'uk', 'es'). Default: en"
    )

    # Optional Argument: ASR Model Type
    parser.add_argument(
        "--model", "-m", 
        choices=["whisper", "google"], 
        default="whisper",
        help="ASR Model to use (whisper or google). Default: whisper"
    )

    # Optional Argument: Whisper Model Size
    parser.add_argument(
        "--size", "-s", 
        choices=["tiny", "base", "small", "medium", "large"], 
        default="base", 
        help="Whisper model size. Ignored if model is google. Default: base"
    )

    # Optional Argument: Output File Path
    parser.add_argument(
        "--output", "-o", 
        help="Path to save the transcript text file. If omitted, prints to stdout."
    )

    # Optional Flag: Verbose/Debug (Example flag)
    parser.add_argument(
        "--verbose", "-v", 
        action="store_true", 
        help="Enable verbose logging"
    )

    args = parser.parse_args()

    # 2. Execution Logic
    transcript = ""
    
    try:
        if args.verbose:
            print(f"Debug: Starting transcription for {args.url} using {args.model}...", file=sys.stderr)

        if args.model == "whisper":
            # Pass the specific size string (e.g., "small") rather than a dictionary
            transcript = whisper_transcribe.transcribe_whisper(
                model_size=args.size, 
                url=args.url, 
                lang=args.lang
            )
        
        elif args.model == "google":
            transcript = google_transcribe.transcribe_google(args.url)
        
        else:
            # This is technically caught by argparse choices, but good for safety
            send_error(f"ASR model '{args.model}' is not supported.")

        # 3. Handle Output (File vs Stdout)
        response = {
            "status": "ok",
            "transcript_preview": transcript[:100] + "..." if len(transcript) > 100 else transcript
        }

        if args.output:
            # Ensure directory exists
            output_dir = os.path.dirname(args.output)
            if output_dir and not os.path.exists(output_dir):
                os.makedirs(output_dir)
            
            with open(args.output, "w", encoding="utf-8") as f:
                f.write(transcript)
            
            response["message"] = f"Transcript saved to {args.output}"
            response["file_path"] = args.output
        else:
            # If no file requested, include full transcript in JSON output
            response["transcript"] = transcript

        print(json.dumps(response))

    except Exception as e:
        send_error(str(e))

if __name__ == "__main__":
    main()
