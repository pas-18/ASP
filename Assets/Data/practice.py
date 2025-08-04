import pygame
from pygame.locals import *
from OpenGL.GL import *
from OpenGL.GLU import *
from OpenGL.GLUT import *
import numpy, math
import logging as log
# import socket
import pickle
import get_E
import calculate
# import dearpygui.dearpygui as dpg
# import win32gui


scale = 1E8
att = False
dragging = False
last_pos = [0, 0]
dt = 10000
dev_mode = False
running = True


"""def move_window(window_title, window_title_2):
    # 查找窗口
    hwnd = win32gui.FindWindow(None, window_title)
    hwnd_2 = win32gui.FindWindow(None, window_title_2)
    if hwnd == 0 or hwnd_2 == 0:
        log.info("Window not found")
        return

        # 获取当前窗口位置
    rect_2 = win32gui.GetWindowRect(hwnd_2)
    width_2 = rect_2[2] - rect_2[0]
    height_2 = rect_2[3] - rect_2[1]

    rect = win32gui.GetWindowRect(hwnd)

    # 计算新的窗口位置（左上角坐标）
    # 注意：这里我们假设你不改变窗口大小，只改变位置
    new_x = rect[2] - 15
    new_y = rect[1]

    # 移动窗口
    win32gui.MoveWindow(hwnd_2, new_x, new_y, width_2, height_2, True)"""


def draw_axis(axis_length):
    draw_line([1.0, 0.0, 0.0], [0, 0, 0], [axis_length, 0, 0])
    draw_line([0.5, 0.0, 0.0], [0, 0, 0], [-axis_length, 0, 0])
    draw_line([0.0, 1.0, 0.0], [0, 0, 0], [0, axis_length, 0])
    draw_line([0.0, 0.5, 0.0], [0, 0, 0], [0, -axis_length, 0])
    draw_line([0.0, 0.0, 1.0], [0, 0, 0], [0, 0, axis_length])
    draw_line([0.0, 0.0, 0.5], [0, 0, 0], [0, 0, -axis_length])


def draw_sphere(center, radius, slices, stacks):
    glPushMatrix()
    glTranslatef(center[0], center[1], center[2])  # 移动球体到指定中心位置
    # glRotatef(90.0, 1.0, 0.0, 0.0)
    glutWireSphere(radius, slices, stacks)  # 绘制球体
    glPopMatrix()


def draw_solid_sphere(center, radius, slices, stacks):
    glPushMatrix()
    glTranslatef(center[0], center[1], center[2])  # 移动球体到指定中心位置
    # glRotatef(90.0, 1.0, 0.0, 0.0)
    glutSolidSphere(radius, slices, stacks)  # 绘制球体
    glPopMatrix()


def draw_line(color, start, end):
    # 设置线的颜色
    glColor3f(color[0], color[1], color[2])

    glBegin(GL_LINES)
    glVertex3f(start[0], start[1], start[2])
    glVertex3f(end[0], end[1], end[2])
    glEnd()


def R_z(theta):
    return numpy.array([[numpy.cos(theta), -numpy.sin(theta), 0],
                        [numpy.sin(theta), numpy.cos(theta), 0],
                        [0, 0, 1]])


def R_x(theta):
    return numpy.array([[1, 0, 0],
                        [0, numpy.cos(theta), -numpy.sin(theta)],
                        [0, numpy.sin(theta), numpy.cos(theta)]])


def draw_ellipse_orbit(a, e, i, Omega, omega, nu, num_segments, colour_start, colour_end, center_object):
    # 绘制椭圆轨道
    glBegin(GL_LINE_STRIP)
    # nu = math.radians(nu)
    # 计算半短轴
    b = a * (1 - e ** 2) ** 0.5
    if e != 0:
        nu = get_E.E_from_f_and_e(nu, e)
    # 创建从近地点开始的参数t的数组
    t = numpy.linspace(nu, 2 * numpy.pi + nu, num_segments)

    # 椭圆的参数方程（未旋转）
    c = a * e
    x_prime = a * numpy.cos(t) - c

    y_prime = b * numpy.sin(t)
    z_prime = numpy.zeros_like(t)  # 因为椭圆在xy平面上

    R = R_z(Omega) @ R_x(i) @ R_z(omega)

    # 将椭圆上的点（在xz平面上）转换为齐次坐标
    # 并应用旋转矩阵
    points_homogeneous = numpy.vstack((x_prime, y_prime, z_prime, numpy.ones_like(t))).T
    points_3d = points_homogeneous[:, :3]
    rotated_points = numpy.dot(R, points_3d.T).T

    x, y, z = rotated_points.T

    if center_object == "Kerbol":
        for i in range(num_segments):
            colour = [(start + (end - start) * i / num_segments) / 255 for start, end in zip(colour_start, colour_end)]
            glColor3f(colour[0], colour[1], colour[2])
            glVertex3f(x[i] / scale, y[i] / scale, z[i] / scale)
        glEnd()
    else:
        for planet in planet_objects:
            if planet.name == center_object:
                center_pos = planet.pos
                for i in range(num_segments):
                    colour = [(start + (end - start) * i / num_segments) / 255 for start, end in
                              zip(colour_start, colour_end)]
                    glColor3f(colour[0], colour[1], colour[2])
                    glVertex3f((x[i] + center_pos[0]) / scale, (y[i] + center_pos[1]) / scale,
                               (z[i] + center_pos[2]) / scale)

                glEnd()
    return [x[0], y[0], z[0]]


def distance(point_a, point_b):
    return ((point_a[0] - point_b[0]) ** 2 + (point_a[1] - point_b[1]) ** 2 + (point_a[2] - point_b[2]) ** 2) ** (1 / 2)


def distance_2d(point_a, point_b):
    return ((point_a[0] - point_b[0]) ** 2 + (point_a[1] - point_b[1]) ** 2) ** (1 / 2)


def update_camera(camera_pos, target, up_vector):
    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()  # 重置模型视图矩阵
    gluLookAt(camera_pos[0], camera_pos[1], camera_pos[2],
              target[0], target[1], target[2],
              up_vector[0], up_vector[1], up_vector[2])


def key_check(camera_pos, move_speed, rotation_speed, get):
    global w, s, a, d, theta, phi, focus_index, att, dev_mode, dragging, last_pos, running
    for event in get:
        if event.type == pygame.QUIT:
            running = False
        if event.type == pygame.KEYDOWN:
            r = distance(camera_pos, focus_pos)
            if event.key == pygame.K_w:
                if theta - rotation_speed > 0:
                    camera_pos[0] = r * numpy.sin(theta - rotation_speed) * numpy.cos(phi) + focus_pos[0]
                    camera_pos[1] = r * numpy.sin(theta - rotation_speed) * numpy.sin(phi) + focus_pos[1]
                    camera_pos[2] = r * numpy.cos(theta - rotation_speed) + focus_pos[2]
            elif event.key == pygame.K_s:
                if theta + rotation_speed < 3.14:
                    camera_pos[0] = r * numpy.sin(theta + rotation_speed) * numpy.cos(phi) + focus_pos[0]
                    camera_pos[1] = r * numpy.sin(theta + rotation_speed) * numpy.sin(phi) + focus_pos[1]
                    camera_pos[2] = r * numpy.cos(theta + rotation_speed) + focus_pos[2]
            if event.key == pygame.K_a:
                camera_pos[0] = r * numpy.sin(theta) * numpy.cos(phi - rotation_speed) + focus_pos[0]
                camera_pos[1] = r * numpy.sin(theta) * numpy.sin(phi - rotation_speed) + focus_pos[1]
                camera_pos[2] = r * numpy.cos(theta) + focus_pos[2]
            elif event.key == pygame.K_d:
                camera_pos[0] = r * numpy.sin(theta) * numpy.cos(phi + rotation_speed) + focus_pos[0]
                camera_pos[1] = r * numpy.sin(theta) * numpy.sin(phi + rotation_speed) + focus_pos[1]
                camera_pos[2] = r * numpy.cos(theta) + focus_pos[2]

            if not att:
                if event.key == 91:
                    if focus_index == 0:
                        focus_index = len(object_list) - 1
                    else:
                        focus_index -= 1
                elif event.key == 93:
                    if focus_index + 1 == len(object_list):
                        focus_index = 0
                    else:
                        focus_index += 1
                att = True
            if not dev_mode:
                if event.key == 92:
                    dev_mode = not dev_mode
        if event.type == pygame.KEYUP:
            if event.key == 91 or event.key == 93:
                att = False
            if event.key == 92:
                dev_mode = False
        if event.type == pygame.MOUSEBUTTONDOWN:
            r = distance(camera_pos, focus_pos)
            if event.button == 5:
                r += move_speed * r
                camera_pos[0] = r * numpy.sin(theta) * numpy.cos(phi) + focus_pos[0]
                camera_pos[1] = r * numpy.sin(theta) * numpy.sin(phi) + focus_pos[1]
                camera_pos[2] = r * numpy.cos(theta) + focus_pos[2]
            elif event.button == 4:
                if (distance(camera_pos, focus_pos) - move_speed) > 0.006:
                    r -= move_speed * r
                    camera_pos[0] = r * numpy.sin(theta) * numpy.cos(phi) + focus_pos[0]
                    camera_pos[1] = r * numpy.sin(theta) * numpy.sin(phi) + focus_pos[1]
                    camera_pos[2] = r * numpy.cos(theta) + focus_pos[2]
            if event.button == 3:  # 鼠标右键
                dragging = True
                last_pos = pygame.mouse.get_pos()
        # 检测鼠标右键释放
        elif event.type == pygame.MOUSEBUTTONUP:
            if event.button == 3:
                dragging = False

        if dragging:
            d_pos = [x - y for x, y in zip(pygame.mouse.get_pos(), last_pos)]
            last_pos = pygame.mouse.get_pos()
            r = distance(camera_pos, focus_pos)

            if d_pos[0] != 0:
                camera_pos[0] = \
                    r * numpy.sin(theta) * numpy.cos(phi - rotation_speed * d_pos[0] * drag_speed) + focus_pos[0]
                camera_pos[1] = \
                    r * numpy.sin(theta) * numpy.sin(phi - rotation_speed * d_pos[0] * drag_speed) + focus_pos[1]
                camera_pos[2] = r * numpy.cos(theta) + focus_pos[2]

                theta = numpy.arccos((camera_pos[2] - focus_pos[2]) / distance(camera_pos, focus_pos))
                phi = numpy.arctan2(camera_pos[1] - focus_pos[1], (camera_pos[0] - focus_pos[0]))

            if (((d_pos[1] > 0) and (theta - rotation_speed * d_pos[1] * drag_speed > 0)) or
                    ((d_pos[1] < 0) and (theta - rotation_speed * d_pos[1] * drag_speed < 3.14))):
                if theta - rotation_speed * d_pos[1] * drag_speed > 0:
                    camera_pos[0] =\
                        r * numpy.sin(theta - rotation_speed * d_pos[1] * drag_speed) * numpy.cos(phi) + focus_pos[0]
                    camera_pos[1] =\
                        r * numpy.sin(theta - rotation_speed * d_pos[1] * drag_speed) * numpy.sin(phi) + focus_pos[1]
                    camera_pos[2] = r * numpy.cos(theta - rotation_speed * d_pos[1] * drag_speed) + focus_pos[2]


def draw_gl_line(start, end, colour):
    glBegin(GL_LINE_STRIP)
    colour = [x / 255 for x in colour]
    glColor3f(colour[0], colour[1], colour[2])
    glVertex3f(start[0], start[1], start[2])
    glVertex3f(end[0], end[1], end[2])
    glEnd()


def draw_gl_line_(start, end):
    glVertex3f(start[0], start[1], start[2])
    glVertex3f(end[0], end[1], end[2])


"""def ctts(message):
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s_:
            s_.connect(('115.236.153.170', 23930))
            s_.sendall(message.encode())
            data = b''
            data += s.recv(1024)
            data = pickle.loads(data)
            return data
    except:
        pass"""


G = 6.67259e-11


# def kepler_predict2(name, M, R_pos, V):
def kepler_predict2(name, M, r, v):
    mu = G * M
    # 计算基础参数
    h = numpy.cross(r, v)
    e_vec = (numpy.cross(v, h) / mu) - r / numpy.linalg.norm(r)
    e = numpy.linalg.norm(e_vec)
    energy = numpy.dot(v, v) / 2 - mu / numpy.linalg.norm(r)
    a = -mu / (2 * energy) if energy < 0 else numpy.inf

    # 轨道倾角
    i = numpy.arccos(h[2] / numpy.linalg.norm(h))
    '''
    # 升交点赤经
    n = numpy.cross([0, 0, 1], h)
    Omega = numpy.arctan2(n[1], n[0]) % (2 * numpy.pi)
    '''
    # 计算升交点赤经 Ω
    n = numpy.cross([0, 0, 1], h)  # 节点线矢量 n = k̂ × h
    n_norm = numpy.linalg.norm(n)

    if n_norm > 1e-6:  # 避免数值误差导致的除零
        Omega = numpy.arctan2(n[1], n[0]) % (2 * numpy.pi)
    else:
        Omega = 0.0  # 赤道轨道约定为 0

    # 近心点幅角
    if numpy.linalg.norm(n) > 1e-6:
        omega = numpy.arccos(numpy.dot(n, e_vec) / (numpy.linalg.norm(n) * e))
        if e_vec[2] < 0:
            omega = 2 * numpy.pi - omega
    else:
        omega = 0  # 赤道轨道特殊处理

    # 真近点角
    nu = numpy.arccos(numpy.dot(e_vec, r) / (e * numpy.linalg.norm(r)))
    if numpy.dot(r, v) < 0:
        nu = 2 * numpy.pi - nu

    return a, e, i, Omega, omega, nu
#     R = numpy.array(R_pos)
#     V = numpy.array(V)
#     r = numpy.sqrt(numpy.dot(R, R))
#
#     H = numpy.cross(R, V)
#     h = numpy.linalg.norm(H)
#     p = h ** 2 / mu
#     X = numpy.array([1, 0, 0])
#     Y = numpy.array([0, 1, 0])
#     Z = numpy.array([0, 0, 1])
#
#     N = numpy.cross(Z, H)
#     n = numpy.linalg.norm(N)
#
#     if 2 / r - numpy.dot(V, V) / mu != 0:
#         a = 1 / abs(2 / r - numpy.dot(V, V) / mu)
#     else:
#         a = None
#
#     E = (numpy.dot(V, V) / mu - 1 / r) * R - numpy.dot(R, V) / mu * V
#
#     e = numpy.sqrt(numpy.dot(E, E))
#
#     if e < 1E-7:
#         e = 0.0
#
#     i = numpy.arccos(numpy.dot(Z, H) / h)
#
#     # 计算升交点赤经
#     Omega = numpy.arccos(numpy.dot(N, X) / n)
#     if numpy.dot(Y, N) < 0.01:
#         Omega = 2 * numpy.pi - Omega
#
#     if e != 0:
#         omega = numpy.arccos(numpy.dot(N, E) / n / e)
#         if numpy.dot(Z, E) < -0.0:
#             omega = 2 * numpy.pi - omega
#     else:
#         omega = 0
#
#     if omega < 1E-7:
#         omega = 0.0
#
#     Omega = numpy.nan_to_num(Omega, nan=0.0)
#     omega = numpy.nan_to_num(omega, nan=0.0)
#
#     if e != 0:
#         # 计算真近点角
#         nu = numpy.arccos(numpy.dot(E, R) / e / r)
#         if numpy.dot(R, V) < 0:
#             nu = 2 * numpy.pi - nu
#
#     else:
#         nu = numpy.arctan2(R_pos[1], R_pos[0])
#         if Omega > 0:
#             nu -= Omega
#
#     return a, e, i, Omega, omega, nu


camera_pos = [-135.9, 0, 0.1]  # 初始相机位置
up_vector = [0.0, 0.0, 1.0]  # 初始上方向
focus_pos = [-136, 0.0, 0.0]

object_mass_dictionary = {
    "Kerbol": 1.7565459E28,

    "Moho": 2.5263314E21,

    "Eve": 1.2243980E23,
        "Gilly": 1.2420363E17,

    "Kerbin": 5.2915158E22,
        "Mun": 9.7599066E20,
        "Minmus": 2.6457580E19,

    "Duna": 4.5154270E21,
        "Ike": 2.7821615E20,

    "Dres": 3.2190937E20,

    "Jool": 4.2332127E24,
        "Laythe": 2.9397311E22,
        "Vall": 3.1087655E21,
        "Tylo": 4.2332127E22,
        "Bop": 3.7261090E19,
        "Pol": 1.0813507E19,

    "Eeloo": 1.1149224E21
}
# tra_length = 870
tra_length = 360 / numpy.tan(numpy.radians(22.5))


class Object:

    def __init__(self, name, colour, radius, velocity, pos, center_object):
        global focus_pos
        self.name = name
        self.colour = colour
        self.mass = object_mass_dictionary[name]
        self.radius = radius
        self.v = velocity
        self.pos = pos
        self.center_object = center_object
        self.a, self.e, self.i, self.Omega, self.omega, self.nu = kepler_predict2(
            self.name, object_mass_dictionary[self.center_object], self.pos, self.v)
        self.kepler = [self.a, self.e, numpy.degrees(self.i),
                       numpy.degrees(self.Omega), numpy.degrees(self.omega), self.nu]
        self.size = self.radius[0] / scale
        self.deg_size = numpy.arctan(self.size / distance(camera_pos, focus_pos))
        if self.center_object == "Kerbol":
            self.size_limit = 0.25
        else:
            self.size_limit = 0.2
            for center in planet_objects:
                if center.name == center_object:
                    self.pos = [x + y for x, y in zip(self.pos, center.pos)]
                    self.v = [x + y for x, y in zip(self.v, center.v)]
        self.acc = [0, 0, 0]
        self.list = []
        self.vis_deg = [0, 0]
        self.screen_pos = [0, 0]

    def draw(self):
        global focus_pos

        if self.center_object == "Kerbol":
            self.a, self.e, self.i, self.Omega, self.omega, self.nu = kepler_predict2(
                self.name, object_mass_dictionary[self.center_object], self.pos, self.v)
            draw_ellipse_orbit(self.a, self.e, self.i, self.Omega, self.omega, self.nu,
                               500, self.colour[1], self.colour[2], self.center_object)
            if dev_mode:
                draw_gl_line([x / scale for x in self.pos], [x / scale + y * 1E-5 for x, y in zip(self.pos, self.v)],
                             [0, 255, 18])

        else:
            for center in planet_objects:
                if center.name == self.center_object:
                    self.a, self.e, self.i, self.Omega, self.omega, self.nu = kepler_predict2(
                        self.name,
                        object_mass_dictionary[self.center_object],
                        [x - y for x, y in zip(self.pos, center.pos)],
                        [x - y for x, y in zip(self.v, center.v)])
                    draw_ellipse_orbit(self.a, self.e, self.i, self.Omega, self.omega, self.nu,
                                       500, self.colour[1], self.colour[2], self.center_object)
                    if dev_mode:
                        draw_gl_line([x / scale for x in self.pos],
                                     [x / scale + y * 1E-4 for x,
                                     y in zip(self.pos, [x - y for x, y in zip(self.v, center.v)])], [0, 255, 18])

        self.deg_size = numpy.arctan(self.radius[0] / scale / distance(camera_pos, [pos / scale for pos in self.pos]))
        if math.degrees(self.deg_size) > self.size_limit:
            self.size = self.radius[0] / scale
        else:
            self.size = numpy.tan(math.radians(self.size_limit)) * distance(camera_pos,
                                                                            [pos / scale for pos in self.pos])
        glColor3f(self.colour[0][0] / 255, self.colour[0][1] / 255, self.colour[0][2] / 255)
        draw_sphere([pos / scale for pos in self.pos],
                    self.size, slices=16, stacks=16)
        if dev_mode:
            draw_gl_line([x / scale for x in self.pos],
                         [x / scale + y * 2.5 for x, y in zip(self.pos, self.acc)], [255, 0, 0])
            glColor3f(1.0, 1.0, 1.0)
            draw_sphere([pos / scale for pos in self.pos], self.radius[1] / scale, slices=16, stacks=16)
            if len(self.list) >= 2:
                for i in range(len(self.list)):
                    if self.center_object == "Kerbol" and (i + 2) < len(self.list):
                        draw_gl_line([x / scale for x in self.list[i]],
                                     [x / scale for x in self.list[i + 1]], [255, 255, 255])
                    else:
                        for center in planet_objects:
                            if center.name == self.center_object:
                                # draw_gl_line()
                                pass
        self.acc = [0, 0, 0]
        """theta = numpy.arccos((camera_pos[2] - focus_pos[2]) / distance(camera_pos, focus_pos))
        phi = numpy.arctan2(camera_pos[1] - focus_pos[1], (camera_pos[0] - focus_pos[0]))"""
        self.vis_deg = [theta - numpy.arccos((camera_pos[2] - self.pos[2] / scale)
                        / distance(camera_pos, [pos / scale for pos in self.pos])),
                        numpy.arctan2(self.pos[1] / scale - camera_pos[1],
                                      (self.pos[0] / scale - camera_pos[0])) - phi + numpy.pi]
        self.screen_pos = [640 - tra_length * numpy.tan(self.vis_deg[1]),
                           360 + tra_length * numpy.tan(self.vis_deg[0])]

    def pos_update(self):
        self.v = [x + y * dt for x, y in zip(self.v, self.acc)]
        self.pos = [x + y * dt for x, y in zip(self.pos, self.v)]
        self.list.append(self.pos)


def ui_correction():
    glTranslatef(camera_pos[0] - numpy.cos(phi) * numpy.sin(theta) * tra_length,
                 camera_pos[1] - numpy.sin(phi) * numpy.sin(theta) * tra_length,
                 camera_pos[2] - numpy.cos(theta) * tra_length)
    glRotatef(numpy.degrees(r_deg[1]), 0, 0, 1)
    glRotatef(numpy.degrees(r_deg[0]), 0, 1, 0)
    glRotatef(90, 0, 0, 1)


def ui_rect(colour, pos, size):
    glBegin(GL_LINE_STRIP)
    colour = [x / 255 for x in colour]
    glColor3f(colour[0], colour[1], colour[2])
    pos[0] -= 640
    pos[1] += 360
    A = [pos[0], pos[1], 0]
    B = [pos[0] + size[0], pos[1], 0]
    C = [pos[0] + size[0], pos[1] - size[1], 0]
    D = [pos[0], pos[1] - size[1], 0]

    draw_gl_line_(A, B)
    draw_gl_line_(B, C)
    draw_gl_line_(C, D)
    draw_gl_line_(D, A)
    glEnd()


def ui_circle(colour, center_pos, radius, debug=0):
    colour = [x / 255 for x in colour]
    glColor3f(colour[0], colour[1], colour[2])
    center_pos[1] *= -1
    center_pos[0] -= 640
    center_pos[1] += 360
    cir_pos = []

    ang = 0.0
    while ang < 360:
        cir_pos.append([center_pos[0] + numpy.sin(numpy.radians(ang)) * radius,
                        center_pos[1] + numpy.cos(numpy.radians(ang)) * radius, 0])
        ang += 10

    i = 0
    while i < len(cir_pos) - 1:
        glBegin(GL_LINE_STRIP)
        draw_gl_line_(cir_pos[i], cir_pos[i + 1])
        glEnd()
        i += 2


def ui_text(colour, pos, text):
    colour = [x / 255 for x in colour]
    glColor3f(colour[0], colour[1], colour[2])
    pos[1] *= -1
    pos[0] -= 640
    pos[1] += 360
    glRasterPos3f(pos[0], pos[1], 0.0)  # 设置文本在窗口中的位置
    for char in text:
        glutBitmapCharacter(GLUT_BITMAP_HELVETICA_12, ord(char))
    # glFlush()


# log.basicConfig(level=log.DEBUG, format='%(asctime)s - %(levelname)s - %(message)s')
log.basicConfig(level=log.DEBUG, format='%(message)s')

pygame.init()
glutInit()
pygame.key.set_repeat(25, 20)

glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH)

clock = pygame.time.Clock()

move_speed = 0.1  # 相机移动速度
rotation_speed = 0.02  # 相机环绕速度
drag_speed = 0.125

screen = pygame.display.set_mode((1280, 720), DOUBLEBUF | OPENGL)
#  name, colour, radius, velocity, pos, center_object
planet_objects = [
    Object("Moho", [[156, 119, 89], [97, 74, 57], [238, 182, 136]], [2.5E5, 9646663],
           [12054.255229311595, -1048.378238435603, -1434.843331723237],
           [-568676230.8703313, -6286981001.916028, -198406747.53784233], "Kerbol"),
    Object("Eve", [[71, 21, 150], [51, 16, 108], [108, 32, 228]], [7E5, 85109365],
           [2779.306953805442, -10438.374968565175, -396.0925886684536],
           [-9596618668.603909, -2555365843.812206, 568132.4366227849], "Kerbol"),
    Object("Kerbin", [[91, 132, 128], [59, 86, 83], [138, 202, 194]], [6E5, 84159286],
           [-14.785336365663886, -9283.452446402018, 0.0],
           [-13599823007.697136, 21659825.247473374, 0.0], "Kerbol"),
    Object("Duna", [[107, 41, 26], [99, 59, 50], [163, 43, 16]], [3.2E5, 47921949],
           [5016.265268570794, 5089.119290546496, -7.483028688854879],
           [15514867153.114445, -15290394870.783937, 32847.33960984683], "Kerbol"),
    Object("Dres", [[59, 45, 33], [38, 28, 21], [90, 68, 50]], [1.36E5, 32832840],
           [797.4888207049592, -4560.119339639848, -0.5672099767916821],
           [-45885132852.90954, -8033714177.403799, -4075490922.914658], "Kerbol"),
    Object("Jool", [[55, 87, 13], [33, 50, 8], [84, 132, 18]], [6E6, 2455985200],
           [-3680.232424215697, 2296.1293725822648, 98.19288140755731],
           [34304864994.34764, 55626280359.17522, 164220541.89217818], "Kerbol"),
    Object("Eeloo", [[68, 69, 69], [42, 43, 43], [104, 106, 106]], [2.1E5, 119082940],
           [-2117.3530912599904, -1775.5245948781503, 51.796477329538504],
           [-72411124161.08376, 86641166587.73544, 11977915803.28477], "Kerbol")
]

planet_objects_2 = [
    Object("Gilly", [[107, 82, 72], [71, 55, 49], [162, 126, 110]], [1.3E4, 126123.27],
           [-57.79730906116456, -542.38141965757, -7.920772981107562],
           [-25598978.390833464, -13483522.926685533, 4860888.6717070155], 'Eve'),

    Object("Mun", [[103, 105, 119], [75, 77, 87], [156, 160, 180]], [2E5, 2429559.1],
           [-537.9123905415701, -69.88959293917317, 0.0],
           [-1546133.9315462955, 11899977.725429622, 0.0], 'Kerbin'),
    Object("Minmus", [[94, 76, 105], [60, 50, 68], [142, 116, 160]], [6E4, 2247428.4],
           [-59.00292924003776, -267.66071266206524, 0.21691226432229582],
           [-45645798.21213556, 10066107.197584258, 4912696.964371395], 'Kerbin'),

    Object("Ike", [[87, 90, 101], [61, 64, 71], [132, 138, 154]], [1.3E5, 1049598.9],
           [-301.5539384001814, -48.279427009169645, -0.16852767694402862],
           [-602109.3546146215, 3158282.1775350976, 11024.529317894083], 'Duna'),

    Object("Laythe", [[45, 56, 103], [29, 36, 64], [68, 86, 156]], [5E5, 3723645.8],
           [-5.133891970043577, -3223.480399103162, 0.0],
           [-27183965.523281433, 43294.67688177794, 0.0], 'Jool'),
    Object("Vall", [[72, 101, 119], [52, 72, 84], [110, 154, 180]], [3E5, 2406401.4],
           [-2004.1251906236928, 1590.3758454391339, 0.0],
           [26823713.35081571, 33802122.80424517, 0.0], 'Jool'),
    Object("Tylo", [[139, 111, 112], [91, 74, 74], [210, 170, 170]], [6E5, 10856518],
           [-3.234134824516856, -2030.656134432824, -0.8860409442717322],
           [-68499913.12333645, 109096.71439410951, 47.60242475307087], 'Jool'),
    Object("Bop", [[123, 105, 83], [75, 65, 51], [186, 160, 126]], [6.5E4, 1221060.9],
           [-1624.7403181085751, -265.9589691979265, 5.4165234115497105],
           [-41500709.14720047, 103299143.83423017, 29189399.035468325], 'Jool'),
    Object("Pol", [[145, 149, 113], [95, 98, 74], [220, 228, 172]], [4.4E4, 1042138.9],
           [-1332.2271998431781, 296.3073394691972, 25.461130694579882],
           [11565209.686624907, 163679891.99879667, 12126113.258987214], 'Jool')
]

rel_pos = [-1.0, 0.0, 1.0]
object_list = ["Moho", "Eve", "Kerbin", "Duna", "Dres", "Jool", "Eeloo"]
focus_index = 2
"""dpg.create_context()
dpg.create_viewport(title='Custom Title', width=1280, height=760)
dpg.setup_dearpygui()
dpg.show_style_editor()
dpg.show_metrics()
with dpg.window():
    dpg.add_text("Hello, world")
dpg.show_viewport()"""

while running:
    # dpg.render_dearpygui_frame()
    glEnable(GL_DEPTH_TEST)
    glDepthFunc(GL_LEQUAL)
    for focus in planet_objects:
        if focus.name == object_list[focus_index]:
            focus_pos = [pos / scale for pos in focus.pos]
            camera_pos = [x + y for x, y in zip(rel_pos, focus_pos)]
            print(focus.vis_deg, focus.screen_pos)

    # mods = pygame.key.get_mods()  # 修饰键检测

    # log.info("fps:" + str(int(clock.get_fps())))

    key_check(camera_pos, move_speed, rotation_speed, pygame.event.get())  # 按键检测
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glLoadIdentity()

    update_camera(camera_pos, focus_pos, up_vector)

    if dev_mode:
        draw_axis(32767)

    rel_pos = [x - y for x, y in zip(camera_pos, focus_pos)]

    theta = numpy.arccos((camera_pos[2] - focus_pos[2]) / distance(camera_pos, focus_pos))
    phi = numpy.arctan2(camera_pos[1] - focus_pos[1], (camera_pos[0] - focus_pos[0]))
    r_deg = [theta, phi]

    for planet in planet_objects:
        planet.pos_update()
        planet.draw()

    for planet_2 in planet_objects_2:
        planet_2.pos_update()
        planet_2.draw()

    update_camera(camera_pos, focus_pos, up_vector)

    mouse_pos = pygame.mouse.get_pos()

    choose_circle = ""

    for planet_2 in planet_objects_2:
        if distance_2d(planet_2.screen_pos, [mouse_pos[0], mouse_pos[1]]) < 50:
            choose_circle = planet_2.name

        for planet in planet_objects_2:
            if planet.name == planet_2.name:
                continue
            else:
                a = calculate.next_step(planet.mass, planet.pos, planet_2.pos)
                planet_2.acc = [x + y for x, y in zip(planet_2.acc, a)]
                if dev_mode:
                    draw_gl_line([x / scale for x in planet_2.pos],
                                 [x / scale + y * 2.5E3 for x, y in zip(planet_2.pos, a)], planet.colour[0])
        for planet in planet_objects:
            a = calculate.next_step(planet.mass, planet.pos, planet_2.pos)
            planet_2.acc = [x + y for x, y in zip(planet_2.acc, a)]
            if dev_mode:
                draw_gl_line([x / scale for x in planet_2.pos],
                             [x / scale + y * 2.5 for x, y in zip(planet_2.pos, a)], planet.colour[0])
        a = calculate.next_step(object_mass_dictionary["Kerbol"], [0, 0, 0], planet_2.pos)
        planet_2.acc = [x + y for x, y in zip(planet_2.acc, a)]
        if dev_mode:
            draw_gl_line([x / scale for x in planet_2.pos],
                         [x / scale + y * 2.5 for x, y in zip(planet_2.pos, a)], [255, 255, 200])

    for planet in planet_objects:
        for planet_2 in planet_objects:
            if planet_2.name == planet.name:
                continue
            else:
                a = calculate.next_step(planet_2.mass, planet_2.pos, planet.pos)
                planet.acc = [x + y for x, y in zip(planet.acc, a)]
                if dev_mode:
                    if dev_mode:
                        draw_gl_line([x / scale for x in planet.pos],
                                     [x / scale + y * 1E4 for x, y in zip(planet.pos, a)], planet_2.colour[0])
        for planet_2 in planet_objects_2:
            a = calculate.next_step(planet_2.mass, planet_2.pos, planet.pos)
            planet.acc = [x + y for x, y in zip(planet.acc, a)]
            if dev_mode:
                if dev_mode:
                    draw_gl_line([x / scale for x in planet.pos],
                                 [x / scale + y * 2.5 for x, y in zip(planet.pos, a)], planet_2.colour[0])
        a = calculate.next_step(object_mass_dictionary["Kerbol"], [0, 0, 0], planet.pos)
        planet.acc = [x + y for x, y in zip(planet.acc, a)]
        if dev_mode:
            draw_gl_line([x / scale for x in planet.pos],
                         [x / scale + y * 2.5 for x, y in zip(planet.pos, a)], [255, 255, 200])




    kerbol_deg_size = numpy.arctan(
        2.616E8 / scale / distance(camera_pos, [0, 0, 0]))
    if math.degrees(kerbol_deg_size) < 0.3:
        kerbol_size = numpy.tan(math.radians(0.3)) * distance(camera_pos, [0, 0, 0])
    else:
        kerbol_size = 2.616E8 / scale
    glColor3f(1.0, 1.0, 200 / 255)
    draw_sphere((0, 0, 0), kerbol_size, slices=32, stacks=32)

    glDisable(GL_DEPTH_TEST)
    ui_correction()

    ui_rect([255, 255, 255], [0, 0], [1280, 720])
    for planet in planet_objects:
        if planet.name == object_list[focus_index]:
            ui_circle([255, 255, 255], [640, 360],
                      numpy.tan(planet.deg_size) * tra_length + 10)
            ui_text([255, 255, 255], [640, 360], planet.name)
        elif planet.name == choose_circle:
            ui_circle([255, 255, 255], planet.screen_pos,
                      numpy.tan(planet.deg_size) * tra_length + 10)
    for planet in planet_objects_2:
        if planet.name == choose_circle:
            ui_circle([255, 255, 255], planet.screen_pos,
                      numpy.tan(planet.deg_size) * tra_length + 10)
            ui_text([255, 255, 255], [mouse_pos[0], mouse_pos[1]], planet.name)
            ui_text([255, 255, 255], [mouse_pos[0], mouse_pos[1] + 15], "speed:" + str(planet.v))



    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    gluPerspective(45, (1280 / 720), 0.001, 3276700.0)
    glMatrixMode(GL_MODELVIEW)
    clock.tick(60)
    pygame.display.flip()
    log.info(clock.get_fps())
    # move_window( "pygame window", "Custom Title")
pygame.quit()
