from deepspeech import Model

def transcribe_deepspeech(file_path):
    # Initialize model (calls DS_CreateModel internally)
    model = Model('deepspeech-0.9.3-models.pbmm')
    model.enableExternalScorer('deepspeech-0.9.3-models.scorer')
    # Load audio file
    with wave.open('audio_file.wav', 'rb') as w:
        rate = w.getframerate()
        frames = w.getnframes()
        buffer = w.readframes(frames)
    # Convert audio to numpy array
    audio = np.frombuffer(buffer, np.int16)
    # Perform speech recognition (calls DS_STT internally)
    text = model.stt(audio)
    print(text)
