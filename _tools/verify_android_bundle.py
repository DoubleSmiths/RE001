"""구글 플레이 제출 전 AAB 점검 — 콘솔에 올리기 전에 로컬에서 거절 사유를 먼저 잡는다.

  1. targetSdk / minSdk / versionCode / package (base/manifest/AndroidManifest.xml — protobuf 라 문자열 대신 bundletool 없이 aapt2 로 덤프)
  2. 네이티브 라이브러리 16KB 페이지 정렬 (targetSdk 35+ 필수, 2025-11 부터) — ELF PT_LOAD p_align >= 0x4000
  3. 64비트(arm64-v8a) 포함, 모듈별 압축 크기(기본 모듈 200MB 한도)

사용: python _tools/verify_android_bundle.py Builds/Android/MinimonQuest.aab
"""
import struct
import sys
import zipfile

PAGE_16K = 0x4000


def elf_min_load_align(data):
    if data[:4] != b"\x7fELF":
        return None
    is64 = data[4] == 2
    little = data[5] == 1
    e = "<" if little else ">"
    if is64:
        phoff = struct.unpack_from(e + "Q", data, 0x20)[0]
        phentsize, phnum = struct.unpack_from(e + "HH", data, 0x36)
    else:
        phoff = struct.unpack_from(e + "I", data, 0x1C)[0]
        phentsize, phnum = struct.unpack_from(e + "HH", data, 0x2A)
    aligns = []
    for i in range(phnum):
        off = phoff + i * phentsize
        p_type = struct.unpack_from(e + "I", data, off)[0]
        if p_type != 1:  # PT_LOAD
            continue
        align = struct.unpack_from(e + ("Q" if is64 else "I"), data, off + (0x30 if is64 else 0x1C))[0]
        aligns.append(align)
    return min(aligns) if aligns else None


def main(path):
    ok = True
    with zipfile.ZipFile(path) as z:
        infos = z.infolist()
        modules = {}
        for info in infos:
            mod = info.filename.split("/", 1)[0]
            modules[mod] = modules.get(mod, 0) + info.compress_size
        print("modules (compressed MB):", {k: round(v / 1048576, 1) for k, v in sorted(modules.items())})
        if modules.get("base", 0) > 200 * 1048576:
            print("FAIL base module > 200MB")
            ok = False

        libs = [i for i in infos if i.filename.endswith(".so")]
        abis = sorted({i.filename.split("/")[-2] for i in libs})
        print("ABIs:", abis)
        if "arm64-v8a" not in abis:
            print("FAIL no arm64-v8a")
            ok = False
        for info in libs:
            align = elf_min_load_align(z.read(info.filename))
            flag = "OK" if align is not None and align >= PAGE_16K else "FAIL"
            if flag == "FAIL":
                ok = False
            print(f"  {flag} {info.filename} align=0x{align or 0:x} size={info.file_size // 1024}KB")
    print("RESULT", "PASS" if ok else "FAIL")
    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1]))
