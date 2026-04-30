#!/usr/bin/env python3
"""
migrate_birthdate_encrypted.py
─────────────────────────────────────────────────────────────────────────────
Employees 테이블의 BirthDate (DateOnly, plain) →
BirthDateEncrypted (AES-256-GCM) 변환 스크립트.

순서:
  1. 개발 PC에서 EF Migration 먼저 실행
       dotnet ef migrations add AddBirthDateEncrypted --project ACI-System/ACI.Web
       dotnet ef database update --project ACI-System/ACI.Web
  2. 이 스크립트 실행 → migrate_birthdate_encrypted.sql 생성
       pip install psycopg2-binary pycryptodome --break-system-packages
       python3 migrate_birthdate_encrypted.py
  3. 생성된 SQL 적용
       psql -h 192.168.1.195 -U bpms -d aci-connect_v4 -f migrate_birthdate_encrypted.sql
"""

import base64
import os
import psycopg2
from Crypto.Cipher import AES

# ── 설정 ────────────────────────────────────────────────────────────────────
DB_HOST     = "192.168.1.195"
DB_PORT     = 5432
DB_NAME     = "aci-connect_v4"
DB_USER     = "bpms"
DB_PASS     = "1234"

# appsettings.json Encryption:Key (32-byte, Base64)
NEW_KEY_B64 = "T0xbte3qstMD6Ryeg+UhdOLxXFw7xosjVLzOdKp2U6A="

OUTPUT_FILE = "migrate_birthdate_encrypted.sql"

# ── AES-256-GCM 암호화 (C# EncryptionService 동일 포맷) ────────────────────
# 저장 형식: Base64( nonce[12] || ciphertext[N] || tag[16] )
def encrypt_new(plaintext: str, key: bytes) -> str:
    nonce = os.urandom(12)
    cipher = AES.new(key, AES.MODE_GCM, nonce=nonce, mac_len=16)
    ciphertext, tag = cipher.encrypt_and_digest(plaintext.encode("utf-8"))
    return base64.b64encode(nonce + ciphertext + tag).decode("ascii")

# ── 메인 ────────────────────────────────────────────────────────────────────
def main():
    key = base64.b64decode(NEW_KEY_B64)
    assert len(key) == 32, "Encryption key must be 32 bytes"

    conn = psycopg2.connect(
        host=DB_HOST, port=DB_PORT, dbname=DB_NAME,
        user=DB_USER, password=DB_PASS
    )
    cur = conn.cursor()

    # BirthDate가 있고 BirthDateEncrypted가 아직 비어 있는 레코드만 대상
    cur.execute("""
        SELECT "Id", "BirthDate"
        FROM   "Employees"
        WHERE  "BirthDate"          IS NOT NULL
          AND ("BirthDateEncrypted" IS NULL OR "BirthDateEncrypted" = '')
        ORDER BY "Id"
    """)
    rows = cur.fetchall()
    cur.close()
    conn.close()

    print(f"암호화 대상: {len(rows)}건")

    lines = [
        "-- migrate_birthdate_encrypted.sql",
        f"-- BirthDate (plain DateOnly) → BirthDateEncrypted (AES-256-GCM)",
        f"-- 대상: {len(rows)}건  (BirthDate IS NOT NULL AND BirthDateEncrypted IS NULL)",
        "-- EF Migration (AddBirthDateEncrypted) 실행 후 적용할 것",
        "",
    ]

    for emp_id, birth_date in rows:
        date_str  = birth_date.strftime("%Y-%m-%d")   # "1990-01-15"
        encrypted = encrypt_new(date_str, key)
        escaped   = encrypted.replace("'", "''")      # SQL injection 방어
        lines.append(
            f"UPDATE \"Employees\" SET \"BirthDateEncrypted\" = '{escaped}'"
            f" WHERE \"Id\" = {emp_id};"
        )

    lines += [
        "",
        f"-- 총 {len(rows)}건 완료",
    ]

    with open(OUTPUT_FILE, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    print(f"SQL 파일 생성: {OUTPUT_FILE}")
    print(f"적용 명령어:")
    print(f"  psql -h {DB_HOST} -U {DB_USER} -d {DB_NAME} -f {OUTPUT_FILE}")

if __name__ == "__main__":
    main()
