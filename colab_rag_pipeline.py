# ==============================================================================
# 🕌 AD-DIIN AI: CUSTOM KNOWLEDGE-BASE RAG PIPELINE (Google Colab)
# ==============================================================================
# Instructions:
# 1. Open Google Colab (https://colab.research.google.com)
# 2. Runtime > Change runtime type > T4 GPU (Optional / Recommended)
# 3. Create a folder named 'knowledge_base' and upload your PDF / TXT files.
# 4. Run the code blocks sequentially.
# ==============================================================================

# --- CELL 1: Install Dependencies ---
!pip install -q langchain langchain-community langchain-text-splitters sentence-transformers chromadb pypdf google-generativeai fastapi uvicorn pyngrok

# --- CELL 2: Load and Process Documents ---
import os
from langchain_community.document_loaders import PyPDFDirectoryLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.embeddings import HuggingFaceEmbeddings
from langchain_community.vectorstores import Chroma

# Create directory if it doesn't exist
os.makedirs("knowledge_base", exist_ok=True)
print("👉 Please upload your Islamic books/PDFs to the 'knowledge_base' folder in Colab.")

# Load PDFs from folder
pdf_loader = PyPDFDirectoryLoader("knowledge_base/")
docs = pdf_loader.load()

print(f"Loaded {len(docs)} document pages.")

# Split documents into chunks
text_splitter = RecursiveCharacterTextSplitter(
    chunk_size=700,      # size of each chunk (characters)
    chunk_overlap=150    # overlap to maintain context between chunks
)
chunks = text_splitter.split_documents(docs)
print(f"Created {len(chunks)} chunks for vector search.")

# --- CELL 3: Create Vector Embeddings & Store in ChromaDB ---
# Multilingual embedding model (great for Bengali, English, Arabic)
embedding_model = HuggingFaceEmbeddings(
    model_name="sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
)

# Store into persistent ChromaDB
persist_directory = "./chroma_db"
vector_db = Chroma.from_documents(
    documents=chunks,
    embedding=embedding_model,
    persist_directory=persist_directory
)
print("✅ Vector database successfully built & persisted in './chroma_db'!")

# --- CELL 4: Strict Islamic RAG Query Engine ---
import google.generativeai as genai

# Free Google Gemini API Key (Get from https://aistudio.google.com)
GEMINI_API_KEY = "YOUR_GEMINI_API_KEY_HERE"  # <-- Replace with your key
genai.configure(api_key=GEMINI_API_KEY)
model = genai.GenerativeModel('gemini-1.5-flash')

def ask_ad_diin_ai(query: str, top_k: int = 4):
    # 1. Retrieve top matching chunks
    retriever = vector_db.as_retriever(search_kwargs={"k": top_k})
    relevant_docs = retriever.invoke(query)
    
    # 2. Build context
    context = "\n\n---\n\n".join([doc.page_content for doc in relevant_docs])
    
    # 3. Strict Islamic Prompt
    system_prompt = f"""
You are an authentic Islamic Knowledge Assistant for the "Ad-Diin" platform.
Answer the user's question STRICTLY and ONLY using the provided Context below.

RULES:
1. If the answer is NOT present in the Context, respond with:
   "দুঃখিত, আমার নলেজবেসে এই বিষয়ে সুনির্দিষ্ট তথ্য নেই।" (or in English if asked in English: "I do not have this information in my knowledge base.")
2. DO NOT make up information or use outside unverified facts.
3. Be respectful, clear, and cite relevant context.

Context:
{context}

User Question:
{query}

Answer:
"""
    # 4. Generate response
    response = model.generate_content(system_prompt)
    return {
        "answer": response.text,
        "sources": [doc.metadata.get("source", "Unknown") for doc in relevant_docs]
    }

# --- CELL 5: Test the System ---
query = "নামাজের ফরজ কয়টি ও কি কি?"
result = ask_ad_diin_ai(query)

print("\n--- Question ---")
print(query)
print("\n--- Answer ---")
print(result["answer"])

