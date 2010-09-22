format ELF64

SYS_READ         = 0
SYS_WRITE        = 1
SYS_CLOSE        = 3
SYS_NANOSLEEP    = 35
SYS_SOCKET       = 41
SYS_CONNECT      = 42
SYS_ACCEPT       = 43
SYS_BIND         = 49
SYS_LISTEN       = 50
SYS_CLONE        = 56
SYS_EXIT         = 60
SYS_GETTIMEOFDAY = 96
SYS_EXIT_GROUP   = 231
CLONE_FLAGS      = 3D0F00h ; CLONE_VM|CLONE_FS|CLONE_FILES|CLONE_SIGHAND|CLONE_THREAD|CLONE_SYSVSEM|CLONE_SETTLS|CLONE_PARENT_SETTID|CLONE_CHILD_CLEARTID
stdin  = 0
stdout = 1
stderr = 2

AF_INET     = 2
SOCK_STREAM = 1
INADDR_ANY  = 0

macro extrn [arg] {
forward	extrn arg
}
extrn strcmp, strcpy, strlen, atoi

macro setup [arg] {
forward match wut&>reg, arg \{ lea reg, [wut] \}
        match wut->reg, arg \{ mov reg, wut   \}
        match ~reg, arg \{     xor reg, reg   \}
}
macro call what, [arg] {
common  setup arg
        call what
}
macro syscall what, errcheck, [arg] {
common  setup arg
        mov eax, what
        syscall
        match =check, errcheck \{
                cmp eax, 0
                jl perr
        \}
}
macro clone stakk, proc {
	syscall SYS_CLONE, nocheck, CLONE_FLAGS->edi, stakk&>rsi, ~edx, ~r10d, ~r8d
	cmp eax, 0
	je proc
}

section '.data'
false:      db "false", 0
perrStr:    db "Stuff went bad", 0
tooFewStr:  db "Too few arguments.", 10, 0
helloWorld: db "Hello, world!", 10, 0
noSend:     db "Couldn't send ping.", 10, 0
timeout:    db "Ping timeout, sending new one.", 10, 0
section '.data' writeable
toSend: db "Ping from "
myPort:	times 9 db 0

section '.bss' writeable
buffa:    times 256 db 0
lastPing: times 2 dq 0

listenerStackEnd: times 64 dq 0
listenerStack = $ - 20h ; What do you mean “shouldn't do that” ?
senderStackEnd: times 64 dq 0
senderStack = $ - 20h

section '.text' executable
public main
main:	cmp edi, 5
	jl tooFew
	mov r15, rsi

	call strcpy, [rsi + 8]->rsi, myPort&>rdi

	call strlen, myPort&>rdi
	mov byte [myPort + rax], 10
	call write, myPort&>rdi

	call atoi, [r15 + 8]->rdi, ~eax
	mov r12d, eax

	call atoi, [r15 + 10h]->rdi, ~eax
	mov r13d, eax

	call atoi, [r15 + 18h]->rdi, ~eax
	mov r14d, eax

	call strcmp, [r15 + 20h]->rdi, false&>rsi, ~eax
	mov r15d, eax ; Convert "false" to 0 and "true" to 1.. "True" to -1, though...

	syscall SYS_GETTIMEOFDAY, check, lastPing&>rdi, ~esi
	clone listenerStack, listener

	cmp r15, 0
	je surveillance
	
	clone senderStack, sender

surveillance:
	mov qword [rsp], 5
	mov qword [rsp + 8], 0
.conts:	syscall SYS_NANOSLEEP, nocheck, rsp->rdi, rsp->rsi
	cmp eax, -1
	je .conts

	syscall SYS_GETTIMEOFDAY, check, rsp->rsi, ~esi

	mov rax, [rsp]
	mov rdx, [lastPing]
	add rdx, r14
	cmp rax, rdx
	jle surveillance

	call write, timeout&>rdi
	clone senderStack, sender
	jmp surveillance

listener:
	syscall SYS_SOCKET, check, AF_INET->rdi, SOCK_STREAM->esi, ~edx
	mov r15d, eax

	mov word [rsp], AF_INET
	mov eax, r12d
	bswap eax
	shr eax, 10h
	mov [rsp + 2], ax
	mov dword [rsp + 4], INADDR_ANY
	
	syscall SYS_BIND, check, r15d->edi, rsp->rsi, 16->edx
	syscall SYS_LISTEN, check, r15d->edi, 5->esi
.loop:	syscall SYS_ACCEPT, check, r15d->edi, ~esi, ~edx
	mov r14d, eax

	syscall SYS_READ, check, eax->edi, buffa&>rsi, 255->edx

	lea rdi, [buffa]
	mov byte [rdi + rax], 0
	call write

	syscall SYS_CLOSE, nocheck, r14d->edi
	syscall SYS_GETTIMEOFDAY, nocheck, lastPing&>rdi, ~esi
	clone senderStack, sender
	jmp .loop

sender:
	mov qword [rsp], 1
	mov qword [rsp + 8], 0
.conts:	syscall SYS_NANOSLEEP, nocheck, rsp->rdi, rsp->rsi
	cmp eax, -1
	je .conts

	syscall SYS_SOCKET, check, AF_INET->edi, SOCK_STREAM->esi, ~edx
	mov r15d, eax

	mov word [rsp], AF_INET
	mov eax, r13d
	bswap eax
	shr eax, 10h
	mov [rsp + 2], ax
	mov dword [rsp + 4], 100007fh

	syscall SYS_CONNECT, nocheck, r15d->edi, rsp->rsi, 16->edx
	cmp eax, 0
	jl .err

	call strlen, toSend&>rdi
	syscall SYS_WRITE, nocheck, r15d->edi, toSend&>rsi, eax->edx
	cmp eax, -1
	je .err

	syscall SYS_CLOSE, nocheck, r15d->edi

	jmp exit

.err:	call write, noSend&>rdi
	jmp exit

perr:	lea rdi, [perrStr]
	call write
	jmp exit_all

write:	push rdi
	call strlen
	pop rsi
	syscall SYS_WRITE, nocheck, stdout->edi, eax->edx
	ret

tooFew:	call write, tooFewStr&>rdi
	jmp exit

exit:	syscall SYS_EXIT, nocheck, 1->edi
exit_all:syscall SYS_EXIT_GROUP, nocheck, 1->edi

