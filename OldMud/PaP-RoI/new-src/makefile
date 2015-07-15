CC      = gcc
PROF    =
OPTOMIZ = -O2
DEBUG   = -g3
WARN    = -Wall
#C_FLAGS = $(PROF) $(OPTOMIZ) $(WARN) $(DEBUG)
C_FLAGS = -c -O -Wall -g3
L_FLAGS = -static -g
SYSTEM  =

O_FILES = act_comm.o act_info.o act_move.o act_obj.o act_wiz.o \
          comm.o const.o db.o fight.o handler.o magic.o \
          save.o special.o update.o bit.o mem.o olc.o olc_act.o \
          olc_save.o string.o mob_commands.o mob_prog.o gr_magic.o \
          id.o chat_new.o interp.o marry.o language.o chat_act.o \
          ore_prog.o raceskill.o act_room.o track.o \
	  casino.o cyborg.o


envy: $(O_FILES)
#	@echo "."
	@echo "[1;35mBuilding Persecutions[1;31m And [1;32mParadoxi[0;37m"
	rm -f envy
	$(CC) $(L_FLAGS) -o envy $(O_FILES)
	@echo "Done."

clean:
	rm -f *.o
	rm -f envy
	make

.c.o: merc.h
#	@echo -n "."
	$(CC) -c $(C_FLAGS) $(SYSTEM) $<

1234:
#	@echo "."
	@echo "Updating envy on 1234."
#	rm -f /home/storm/EOS/src/envy
	cp -f envy /home/storm/EOS/src
	@echo "Done."

2222:
#	@echo "."
	@echo "Updating envy on 2222."
#	rm -f /home/storm/EOSBUILD/src/envy
	cp -f envy /home/storm/EOSBUILD/src
	@echo "Done."

port:
#	make
	make 1234
	make 2222
