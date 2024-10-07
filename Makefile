all: build install

build:
  ./make.sh build

clean:
	./make.sh clean

install:
	./make.sh install

test:
	./make.sh build --debug

uninstall:
	./make.sh uninstall