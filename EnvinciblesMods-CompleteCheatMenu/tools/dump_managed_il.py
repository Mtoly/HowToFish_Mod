import sys
from pathlib import Path

import dnfile
from dncil.cil.error import MethodBodyFormatError
from dncil.cil.body.reader import read_method_body_from_bytes


def token_text(pe, token):
    try:
        if token.table == 0x70:
            return repr(pe.net.user_strings.get(token.rid) or "")
        table = pe.net.mdtables.tables.get(token.table)
        if table is None or token.rid < 1 or token.rid > len(table.rows):
            return str(token)
        row = table.rows[token.rid - 1]
        if hasattr(row, "Class") and hasattr(row, "Name"):
            cls = row.Class.row if row.Class else None
            ns = str(getattr(cls, "TypeNamespace", ""))
            name = str(getattr(cls, "TypeName", ""))
            owner = f"{ns}.{name}".strip(".") or str(getattr(cls, "Name", ""))
            return f"{owner}::{row.Name}"
        if hasattr(row, "Name"):
            return str(row.Name)
        return str(row)
    except Exception:
        return str(token)


def main():
    if len(sys.argv) < 4:
        raise SystemExit("usage: dump_managed_il.py ASSEMBLY TYPE METHOD [METHOD ...]")
    pe = dnfile.dnPE(sys.argv[1])
    wanted_type = sys.argv[2]
    wanted_methods = set(sys.argv[3:])
    for typedef in pe.net.mdtables.TypeDef:
        full = f"{typedef.TypeNamespace}.{typedef.TypeName}".strip(".")
        if full != wanted_type and str(typedef.TypeName) != wanted_type:
            continue
        for method_index in typedef.MethodList:
            method = method_index.row
            if str(method.Name) not in wanted_methods:
                continue
            print(f"=== {full}::{method.Name} RVA=0x{method.Rva:X} ===")
            try:
                body = read_method_body_from_bytes(pe.get_data(method.Rva, 0x10000))
            except (MethodBodyFormatError, ValueError) as exc:
                print(f"<no body: {exc}>")
                continue
            for insn in body.instructions:
                operand = insn.operand
                if hasattr(operand, "table") and hasattr(operand, "rid"):
                    operand = token_text(pe, operand)
                print(f"IL_{insn.offset:04X}: {insn.opcode.name:<12} {operand if operand is not None else ''}")


if __name__ == "__main__":
    main()
