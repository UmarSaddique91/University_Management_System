import React from 'react'
import Logo from '../assets/Logo.png'
import { LayoutDashboard, Users, Folder, Search, LogOut, GraduationCap, Landmark, BookOpen, ShieldCheck } from 'lucide-react'
import { useLocation, useNavigate, NavLink } from 'react-router-dom'
import { useState, useEffect, useRef } from 'react'

const Layout = ({ children }) => {
    const location = useLocation();
    const navigate = useNavigate();

    // Checking roles
    const roles = JSON.parse(localStorage.getItem("roles") || "[]");

    const hasAdminAccess = roles.includes("Admin") || roles.includes("SuperAdmin") || roles.includes("Super Admin");
    const isSuperAdmin = roles.includes("SuperAdmin") || roles.includes("Super Admin");
    const isTeacher = roles.includes("Teacher");
    const isStudent = roles.includes("Student");

    // Display Name
    const userName = localStorage.getItem("userName") || (isSuperAdmin ? "Super Admin" : isTeacher ? "Teacher" : isStudent ? "Student" : "Admin");

    // States for Profile Dropdown
    const [showProfile, setShowProfile] = useState(false);
    const profileRef = useRef();

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (profileRef.current && !profileRef.current.contains(event.target)) {
                setShowProfile(false);
            }
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, []);

    // Logout Handler 
    const handleLogout = () => {
        localStorage.removeItem("token");
        localStorage.removeItem("roles");
        localStorage.removeItem("userName");
        navigate("/");
    };
    return (

        <div className="h-screen bg-slate-200 flex overflow-hidden">

            {/* Left Side Bar  */}
            <aside>


                <div className="w-58 bg-slate-900 h-full flex flex-col  p-5">

                    <img src={Logo} alt="University Logo" className='w-50 h-20' />

                    {/* Navigation */}


                    {hasAdminAccess &&(

                        <nav className="flex flex-col flex-1 space-y-1 mt-6">
                            <NavLink
                                to="/dashboard"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <LayoutDashboard className="text-white" size={14}/>
                                <span className="text-[14px]">Dashboard</span>
                            </NavLink>

                            <NavLink
                                to="/teachers"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Users className="text-white" size={14} />
                                <span className="text-[14px]">Teachers</span>
                            </NavLink>

                            <NavLink
                                to="/students"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Users className="text-white" size={14} />
                                <span className="text-[14px]">Students</span>
                            </NavLink>

                            <NavLink
                                to="/departments"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Landmark className="text-white" size={14} />
                                <span className="text-[14px]">Departments</span>
                            </NavLink>

                            <NavLink
                                to="/courses"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Folder className="text-white" size={14} />
                                <span className="text-[14px]">Courses</span>
                            </NavLink>

                            <NavLink
                                to="/academic-programs"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive || location.pathname === '/academicPrograms' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <GraduationCap className="text-white" size={14} />
                                <span className="text-[14px]">Academic Programs</span>
                            </NavLink>

                            <NavLink
                                to="/course-assignments"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <BookOpen className="text-white" size={14} />
                                <span className="text-[14px]">Course Assignments</span>
                            </NavLink>

                            {isSuperAdmin && (
                                <NavLink
                                    to="/admins"
                                    className={({ isActive }) =>
                                        `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                            isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                        }`
                                    }
                                >
                                    <ShieldCheck className="text-white" size={14} />
                                    <span className="text-[14px]">Admins</span>
                                </NavLink>
                            )}
                        </nav>
                    )}

                    {/* Teacher Portal Navigation */}
                    {isTeacher && (
                        <nav className="flex flex-col flex-1 space-y-1 mt-6">
                            <NavLink
                                to="/teacher/courses"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <BookOpen className="text-white" size={14} />
                                <span className="text-[14px]">My Courses</span>
                            </NavLink>

                            <NavLink
                                to="/teacher/exams"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <GraduationCap className="text-white" size={14} />
                                <span className="text-[14px]">Exams & Marks</span>
                            </NavLink>

                            <NavLink
                                to="/teacher/attendance"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Users className="text-white" size={14} />
                                <span className="text-[14px]">Attendance</span>
                            </NavLink>
                        </nav>
                    )}

                    {/* Student Portal Navigation */}
                    {isStudent && (
                        <nav className="flex flex-col flex-1 space-y-1 mt-6">
                            <NavLink
                                to="/student/courses"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <BookOpen className="text-white" size={14} />
                                <span className="text-[14px]">My Courses</span>
                            </NavLink>

                            <NavLink
                                to="/student/results"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <GraduationCap className="text-white" size={14} />
                                <span className="text-[14px]">Results</span>
                            </NavLink>

                            <NavLink
                                to="/student/attendance"
                                className={({ isActive }) =>
                                    `flex items-center text-white gap-2 py-2 px-2 rounded rounded-md w-full transition-colors ${
                                        isActive ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'
                                    }`
                                }
                            >
                                <Users className="text-white" size={14} />
                                <span className="text-[14px]">Attendance</span>
                            </NavLink>
                        </nav>
                    )}

                    
                </div>
            </aside>




            {/* Main Area: Header + Content */}
            <div className="flex-1 flex flex-col min-w-0 h-full overflow-hidden">
                {/* Header */}
                <header className="flex h-14 bg-white w-full items-center justify-between px-6 shadow-sm flex-shrink-0">
                    <div className="relative flex items-center">
                        <Search className="absolute text-gray-400 left-3 w-4 h-4 pointer-events-none" />
                        <input type="text" placeholder="Search teachers, course..."
                            className="bg-slate-100 rounded-md py-1.5 pl-9 pr-4 w-72 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>

                    {/* User profile section */}
                    <div className="flex items-center gap-3">
                        <div 
                            onClick={() => setShowProfile(!showProfile)}
                            ref={profileRef}
                            className="relative flex items-center gap-2 cursor-pointer">
                            <div className="w-8 h-8 rounded-full overflow-hidden bg-slate-200 flex-shrink-0">
                                <img className="w-full h-full object-cover" src={`https://ui-avatars.com/api/?name=${encodeURIComponent(userName)}&background=0D8ABC&color=fff`} alt="user avatar" />
                            </div>

                            <span className="text-black text-sm font-semibold">{userName}</span>


                            {/* Logout Drop Down Box  */}
                            {showProfile && (
                                <div className="absolute top-10 right-0 mt-1 bg-white border border-gray-300 rounded-md w-44 p-2 overflow-hidden shadow-lg z-50">
                                    <div className="flex items-center gap-2 bg-gray-300 px-3 py-2 -mx-2 -mt-2 border-b border-black">
                                        <div className="w-8 h-8 rounded-full overflow-hidden flex-shrink-0">
                                            <img className="w-full h-full object-cover" src={`https://ui-avatars.com/api/?name=${encodeURIComponent(userName)}&background=0D8ABC&color=fff`} alt="user avatar" />
                                        </div>

                                        <div className="flex flex-col min-w-0">
                                            <span className="font-semibold text-sm truncate">{userName}</span>
                                            <span className="text-gray-600 text-xs font-semibold -mt-0.5">{isSuperAdmin ? "Super Administrator" : isTeacher ? "Faculty Teacher" : isStudent ? "Enrolled Student" : "Administrator"}</span>
                                        </div>
                                    </div>

                                    {/* Logout Button  */}
                                    <div>
                                        <button onClick={handleLogout} className='bg-gray-300 w-full px-3 py-2 cursor-pointer rounded-md my-2 flex items-center gap-2'>
                                            <LogOut className="w-4 h-4 text-gray-800"/>
                                            <p className='text-blue-500 font-semibold text-xs'>LogOut</p>
                                        </button>
                                    </div>

                                    {/* Help&Support  */}
                                    <div className='bg-gray-300 border-t border-black px-3 py-2 -mx-2 -mb-2 text-center'>
                                        <a href="#" className='text-blue-500 underline text-xs'>Help&Support</a>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </header>

                {/* Page Body Content */}
                <main className="flex-1 overflow-y-auto p-6">
                    {children}
                </main>
            </div>
        </div>

    )
}

export default Layout