import React from 'react'
import Logo from '../assets/Logo.png'
import { LayoutDashboard, Users, Folder, FileText, Settings, Search, Bell, LogOut } from 'lucide-react'
import { useLocation,useNavigate } from 'react-router-dom'
import { useState, useEffect, useRef } from 'react'

const Layout = ({ children }) => {
    const location = useLocation();
    const navigate=useNavigate();


    //States and Logic For Notofication

    const [showNotification, setShowNotification] = useState(false);
    const [notifications, setNotificaitons] = useState([
        {
            id: 1,
            message: "Assigned Web Development Course to Sara Khan",
            timeStamp: "04/09/2026, 1:22pm"
        },
        {
            id: 2,
            message: "Assigned Web Development Course to Sara Khan",
            timeStamp: "04/09/2026, 1:22pm"
        }
    ])


    // States for Profile

    const [showProfile,setShowProfile]= useState(false);
    const profileRef= useRef();

    // Close notification dropdown when clicking outside
    const notificationRef = useRef(null);



    // UseEffects For Handel DropDowns 

    useEffect(() => {
        const handleClickOutside = (event) => {
            if (notificationRef.current && !notificationRef.current.contains(event.target)) {
                setShowNotification(false);
            }

            if(profileRef.current && !profileRef.current.contains(event.target)){
                setShowProfile(false);
            };
        };

        document.addEventListener('mousedown', handleClickOutside);
        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        }


    })

    
        // Logout Handler 

        const handleLogout=(e)=>{
            navigate("/");
        }
    return (

        <div className="h-screen bg-slate-200 flex overflow-hidden">

            {/* Left Side Bar  */}
            <aside>


                <div className="w-58 bg-slate-900 h-full flex flex-col  p-5">

                    <img src={Logo} alt="University Logo" className='w-50 h-20' />

                    {/* Navigation */}

                    <nav className="flex flex-col flex-1 space-y-1 mt-6">

                        <a href="/dashboard"
                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/dashboard' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <LayoutDashboard className="text-white" size={14}/>
                            <span className="text-[14px]">Dashboard</span>
                        </a>

                        <a href="/teachers"
                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/teachers' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Users className="text-white" size={14} />
                            <span className="text-[14px]">Teachers</span>
                        </a>


                        <a href="/courses"
                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/courses' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Folder className="text-white" size={14} />
                            <span className="text-[14px]">Courses</span>
                        </a>


                        <a href="/assignments"
                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/assignments' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <FileText className="text-white" size={14} />
                            <span className="text-[14px]">Assignments</span>
                        </a>


                        <a href="/settings"

                            className={`flex items-center text-white gap-2 py-2 px-2 
                rounded rounded-md w-full transition-colors 
                ${location.pathname === '/settings' ? 'bg-blue-500' : 'bg-transparent hover:bg-blue-600'}  `}>

                            <Settings className="text-white" size={14} />
                            <span className="text-[14px]">Settings</span>
                        </a>
                    </nav>
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

                    {/* bell and admin icon */}
                    <div className="flex items-center gap-3">

                        {/* Bell Icon for Notifications */}
                        <div className="relative">
                            <div
                                onClick={() => setShowNotification(!showNotification)}
                                ref={notificationRef}
                                className="relative rounded-md bg-slate-100 p-1.5 cursor-pointer hover:bg-slate-200 flex items-center justify-center">
                                <Bell className="text-gray-600 w-5 h-5" />
                                {notifications.length > 0 && (
                                    <span className="absolute top-1 right-1 w-2 h-2 rounded-full bg-red-500"></span>
                                )}
                            </div>


                            {/* Notification DropDown Box */}

                            {showNotification && (
                                <div className="absolute top-10 right-0 mt-1 bg-white border border-gray-300 overflow-hidden rounded-md w-52 p-2 shadow-lg z-50">
                                    <h2 className='font-bold text-sm bg-gray-300 border-b border-black px-3 py-2 -mx-2 -mt-2'>Notifications</h2>


                                    <div>
                                        {notifications.length > 0 ? (
                                            notifications.map((notification) => (
                                                <div key={notification.id} className="px-3 py-2 border-b border-gray-300 -mx-2">
                                                    <div className="font-semibold text-xs">{notification.message}</div>
                                                    <div className='text-slate-500 text-xs'>{notification.timeStamp}</div>
                                                </div>
                                            ))
                                        ) : (
                                            <div className="p-2 text-gray-500 text-xs">No notifications</div>
                                        )}
                                    </div>

                                    <div className='bg-gray-300 border-t border-black px-3 py-2 -mx-2 -mb-2 text-center'>
                                        <a href="#"
                                            onClick={(e) => {
                                                e.preventDefault();
                                                setNotificaitons([])
                                            }}
                                            className='text-blue-500 underline text-xs'>Clear All</a>
                                    </div>

                                </div>


                            )}
                        </div>


                        {/* Admin Icon and Name */}
                        <div 
                            onClick={() => setShowProfile(!showProfile)}
                            ref={profileRef}
                            className="relative flex items-center gap-2 cursor-pointer">
                            <div className="w-8 h-8 rounded-full overflow-hidden bg-slate-200 flex-shrink-0">
                                <img className="w-full h-full object-cover" src="https://ui-avatars.com/api/?name=Admin+User&background=0D8ABC&color=fff" alt="admin image" />
                            </div>

                            <span className="text-black text-sm font-semibold">Admin</span>


                            {/* Logout Drop Down Box  */}

                            {showProfile && (

                                <div className="absolute top-10 right-0 mt-1 bg-white border border-gray-300 rounded-md w-40 p-2 overflow-hidden shadow-lg z-50">
                                    <div className="flex items-center gap-2 bg-gray-300 px-3 py-2 -mx-2 -mt-2 border-b border-black">

                                        <div className="w-8 h-8 rounded-full overflow-hidden flex-shrink-0">
                                            <img className="w-full h-full object-cover" src="https://ui-avatars.com/api/?name=Admin+User&background=0D8ABC&color=fff" alt="admin image" />
                                        </div>


                                        <div className="flex flex-col">
                                            <span className="font-semibold text-sm">Admin User</span>
                                            <span className="text-gray-600 text-xs font-semibold -mt-0.5">Administrator</span>
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