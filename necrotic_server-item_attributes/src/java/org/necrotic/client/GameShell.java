package org.necrotic.client;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Graphics;
import java.awt.Image;
import java.awt.Insets;
import java.awt.Toolkit;
import java.net.URL;

import javax.imageio.ImageIO;
import javax.swing.JFrame;
import javax.swing.JMenuBar;

import org.necrotic.Configuration;

final class GameShell extends JFrame {

	private static final long serialVersionUID = 1L;

	private final GameRenderer applet;

	public GameShell(GameRenderer applet, int width, int height, boolean undecorative, boolean resizable) {
		this.applet = applet;
		setTitle(""+Configuration.CLIENT_NAME+" Client");
		setFocusTraversalKeysEnabled(false);
		setUndecorated(undecorative);
		JMenuBar bar = new JMenuBar();
		add(BorderLayout.NORTH, bar);
		setResizable(resizable);
		setVisible(true);
		Insets insets = getInsets();
		setSize(width + insets.left + insets.right, height + insets.top + insets.bottom);
		setLocationRelativeTo(null);
		requestFocus();
		toFront();
		setBackground(Color.BLACK);
		setClientIcon();
	}
	
	public void setClientIcon() {
		try {
			setIconImage(ImageIO.read(getClass().getResource("favicon.png")));
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public Graphics getGraphics() {
		Graphics g = super.getGraphics();
		Insets insets = getInsets();
		g.translate(insets.left, insets.top);
		return g;
	}

	@Override
	public void paint(Graphics g) {
		applet.paint(g);
	}

	@Override
	public void update(Graphics g) {
		applet.update(g);
	}

}